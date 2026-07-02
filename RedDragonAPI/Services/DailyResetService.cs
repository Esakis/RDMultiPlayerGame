using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class DailyResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyResetService> _logger;

    public DailyResetService(IServiceProvider serviceProvider, ILogger<DailyResetService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            DateTime nextRun;

            if (now.Hour >= 5)
            {
                nextRun = DateTime.Today.AddDays(1).AddHours(5);
            }
            else
            {
                nextRun = DateTime.Today.AddHours(5);
            }

            var delay = nextRun - now;
            _logger.LogInformation("Następne przeliczenie o: {NextRun}. Czekam {Hours:F1} godzin.", nextRun, delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await PerformDailyReset();
        }
    }

    private async Task PerformDailyReset()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var battleService = scope.ServiceProvider.GetRequiredService<IBattleService>();

        _logger.LogInformation("Rozpoczynam przeliczenie codzienne...");

        try
        {
            // 1. Wykonaj zakolejkowane akcje w oryginalnej kolejności faz:
            //    złodziejska → magiczna → wojskowa (potem budowy)
            var phaseOrder = new Dictionary<string, int>
            {
                ["ThiefAction"] = 0,
                ["Spell"] = 1,
                ["MilitaryAttack"] = 2,
                ["Construction"] = 3
            };

            var pendingActions = (await context.QueuedActions
                .Where(a => a.Status == "Pending" && a.ScheduledFor <= DateTime.UtcNow)
                .ToListAsync())
                .OrderBy(a => phaseOrder.GetValueOrDefault(a.ActionType, 9))
                .ThenBy(a => a.CreatedAt)
                .ToList();

            foreach (var action in pendingActions)
            {
                try
                {
                    switch (action.ActionType)
                    {
                        case "MilitaryAttack":
                            await battleService.ExecuteMilitaryAttackAsync(action);
                            break;
                        case "ThiefAction":
                            await battleService.ExecuteThiefActionAsync(action);
                            break;
                        case "Spell":
                            await battleService.ExecuteSpellAsync(action);
                            break;
                        case "Construction":
                            await CompleteConstruction(action, context);
                            break;
                    }

                    action.Status = "Executed";
                    action.ExecutedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd przy wykonywaniu akcji {ActionId}", action.Id);
                    action.Status = "Failed";
                }
            }

            await context.SaveChangesAsync();

            // 2. Generuj zasoby dla wszystkich księstw
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
            await resourceService.GenerateResourcesForAllAsync();

            // 2b. Przychodzenie generałów + powroty z ataków i wyleczenia
            var generalService = scope.ServiceProvider.GetRequiredService<IGeneralService>();
            await generalService.ProcessGeneralArrivalsAsync();

            var outsideGenerals = await context.Generals.Where(g => g.IsOutside).ToListAsync();
            foreach (var general in outsideGenerals)
                general.IsOutside = false;

            // 3. Odnów tury (pomijamy zamrożone gubernaty)
            // Nowicjusz traci ochronę po przekroczeniu obszaru (Kingdom.NoviceLandCap = 30000).
            var kingdoms = await context.Kingdoms
                .Include(k => k.Buildings).ThenInclude(b => b.Definition)
                .Where(k => k.Era.IsActive && !k.IsFrozen && !k.IsSuspended)
                .ToListAsync();

            // Koalicje w aktywnym stanie wojny — Renowacja broni produkuje wtedy broń
            var warringCoalitions = (await context.Wars
                    .Where(w => w.Status == "Active")
                    .Select(w => new { w.DeclaringCoalitionId, w.TargetCoalitionId })
                    .ToListAsync())
                .SelectMany(w => new[] { w.DeclaringCoalitionId, w.TargetCoalitionId })
                .ToHashSet();

            foreach (var kingdom in kingdoms)
            {
                int bonusTurns = kingdom.Buildings
                    .Where(b => !b.IsUnderConstruction && b.Quantity > 0 && b.Definition != null)
                    .Sum(b => b.Definition.BonusTurnsPerDay * b.Quantity);

                // Goblin: Wieża Czasu daje +2 tury zamiast +1 (rebalans 31. wieku)
                if (kingdom.Race == "Goblin" && kingdom.Buildings.Any(b =>
                        b.BuildingType == "ZachodniaWiezaCzasu" && b.Quantity > 0 && !b.IsUnderConstruction))
                {
                    bonusTurns += 1;
                }

                // Badania Czasu (Zakrzywienie/Załamanie) dają tury jednorazowo w chwili
                // odkrycia (ResourceService.InvestScienceAsync), nie co turę.

                int totalTurnsToAdd = kingdom.TurnsPerDay + bonusTurns;

                // „trojtah": kumulacja maksymalnie do potrójnego dziennego przydziału
                int maxTurns = (kingdom.TurnsPerDay + bonusTurns) * 3 + 4;
                kingdom.MaxTurns = maxTurns;

                kingdom.TurnsAvailable = Math.Min(
                    kingdom.TurnsAvailable + totalTurnsToAdd,
                    maxTurns);

                // Nowy cykl: przydział = aktualnie dostępne tury (mianownik licznika 0→max)
                kingdom.TurnsCapacity = kingdom.TurnsAvailable;

                // Nowe przeliczenie — odnów budżet akcji labiryntu (docs/MECHANIKA.md §13)
                kingdom.LabyrinthActionsUsed = 0;

                // Upojenie armii mija — trzeźwieją po przeliczeniu (docs/MECHANIKA.md §10)
                kingdom.DrunkArmyPct = 0;

                // Renowacja broni w czasie wojny (Dracopedia §14.3): 40–50 tys. broni/przeliczenie
                if (kingdom.CoalitionId != null && warringCoalitions.Contains(kingdom.CoalitionId.Value)
                    && kingdom.Buildings.Any(b => b.BuildingType == "RenowacjaBroni"
                                                  && b.Quantity > 0 && !b.IsUnderConstruction))
                {
                    int warWeapons = Random.Shared.Next(40_000, 50_001);
                    kingdom.Weapons += warWeapons;
                    context.KingdomEvents.Add(new KingdomEvent
                    {
                        KingdomId = kingdom.Id,
                        Category = "Economy",
                        Message = $"Renowacja broni pracuje na wojnę: +{warWeapons} broni."
                    });
                }

                kingdom.Age++;
                if (kingdom.IsProtected)
                {
                    kingdom.ProtectionDaysLeft--;
                    // Status nowicjusza kończy się po wyczerpaniu dni lub przekroczeniu obszaru
                    if (kingdom.ProtectionDaysLeft <= 0 || kingdom.Land >= Kingdom.NoviceLandCap)
                        kingdom.IsProtected = false;
                }

                // Zdarzenia losowe — plagi (GAME_DESIGN.md, krok 18 tury). Chronieni nowicjusze
                // są oszczędzani. Odporności rasowe wg docs/MECHANIKA.md §2.2 i §14.4.
                if (!kingdom.IsProtected)
                    await ApplyRandomPlagues(kingdom, context);
            }

            // 4. Zakończ szkolenie jednostek
            var trainingUnits = await context.MilitaryUnits
                .Include(m => m.Definition)
                .Where(m => m.InTraining > 0 && m.TrainingCompletesAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var unit in trainingUnits)
            {
                int trained = unit.InTraining;
                unit.Quantity += unit.InTraining;
                unit.InTraining = 0;
                unit.TrainingCompletesAt = null;

                string name = unit.Definition?.DisplayName ?? unit.UnitType;
                context.KingdomEvents.Add(new KingdomEvent
                {
                    KingdomId = unit.KingdomId,
                    Category = "Training",
                    Message = $"Wyszkolono: {name} ×{trained}"
                });
            }

            // 4b. Automatyczny awans jednostek (Szkolenie żołnierzy/elity, Dracopedia/Trening)
            var trainingKingdoms = await context.Kingdoms
                .Include(k => k.MilitaryUnits)
                .Include(k => k.Researches)
                .Include(k => k.Buildings)
                .Where(k => k.Era.IsActive && !k.IsFrozen && !k.IsSuspended && (k.TrainSoldiers || k.TrainElite))
                .ToListAsync();

            if (trainingKingdoms.Count > 0)
            {
                var races = trainingKingdoms.Select(k => k.Race).Distinct().ToList();
                var defsByRace = (await context.UnitDefinitions
                        .Where(u => races.Contains(u.Race)).ToListAsync())
                    .GroupBy(u => u.Race)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var kingdom in trainingKingdoms)
                {
                    if (!defsByRace.TryGetValue(kingdom.Race, out var defs)) continue;
                    int level = TrainingHelper.TrainingLevel(kingdom.Researches);

                    var hoplitaDef = defs.FirstOrDefault(TrainingHelper.IsHoplita);
                    var e1Def = defs.FirstOrDefault(TrainingHelper.IsElite1);
                    var e2Def = defs.FirstOrDefault(TrainingHelper.IsElite2);

                    bool hasSoldierBld = kingdom.Buildings.Any(b => b.BuildingType == TrainingHelper.SoldierBuilding && b.Quantity > 0 && !b.IsUnderConstruction);
                    bool hasEliteBld = kingdom.Buildings.Any(b => b.BuildingType == TrainingHelper.EliteBuilding && b.Quantity > 0 && !b.IsUnderConstruction);

                    if (kingdom.TrainSoldiers && hasSoldierBld && hoplitaDef != null && e1Def != null)
                        PromoteUnits(context, kingdom, hoplitaDef.UnitType, e1Def, TrainingHelper.SoldierPromotePct(level));

                    if (kingdom.TrainElite && hasEliteBld && e1Def != null && e2Def != null)
                        PromoteUnits(context, kingdom, e1Def.UnitType, e2Def, TrainingHelper.ElitePromotePct(level));
                }
            }

            // 5. Spadek siły zaklęć (oryginalny wzór):
            //    biała magia: nowa = siła·(0,45 + lvlBM/200)·(1+s·0,1) − ziemia/100
            //    czarna magia: nowa = siła·(0,6 − lvlBM/200) − ziemia/100
            var activeSpells = await context.ActiveSpells
                .Include(s => s.Spell)
                .Include(s => s.Kingdom)
                .ToListAsync();

            // Najlepszy generał z Białą magią per księstwo (Dracopedia §9): spowalnia spadek
            // białej magii i przyspiesza spadek czarnej (lvlBM/200).
            var spellKingdomIds = activeSpells.Select(s => s.KingdomId).Distinct().ToList();
            var whiteMagicLevels = (await context.Generals
                    .Where(g => spellKingdomIds.Contains(g.KingdomId) && g.SecondaryTrait == "BialaMagia"
                                && !g.IsImprisoned && !g.IsPending)
                    .ToListAsync())
                .GroupBy(g => g.KingdomId)
                .ToDictionary(grp => grp.Key, grp => grp.Max(g => g.Level));
            // Goblin z Tajemnicą materii (TajemnicaOdtworzenia): wolniejszy spadek białej magii (s=1).
            var tajemnicaKingdoms = (await context.Buildings
                    .Where(b => spellKingdomIds.Contains(b.KingdomId) && b.BuildingType == "TajemnicaOdtworzenia"
                                && b.Quantity > 0 && !b.IsUnderConstruction)
                    .Select(b => b.KingdomId).Distinct().ToListAsync())
                .ToHashSet();

            foreach (var spell in activeSpells)
            {
                bool isPositive = spell.Spell != null &&
                    (spell.Spell.Category == "Biała" || spell.Spell.Category == "Tarcze");
                int lvlBM = whiteMagicLevels.GetValueOrDefault(spell.KingdomId, 0);
                decimal s = tajemnicaKingdoms.Contains(spell.KingdomId) ? 1m : 0m;
                decimal decayFactor = isPositive
                    ? (0.45m + lvlBM / 200m) * (1m + s * 0.1m)
                    : Math.Max(0m, 0.6m - lvlBM / 200m);
                int newPower = (int)(spell.Power * decayFactor - spell.Kingdom.Land / 100);

                if (newPower <= 0 ||
                    (spell.ExpiresAt.HasValue && spell.ExpiresAt <= DateTime.UtcNow))
                {
                    context.ActiveSpells.Remove(spell);
                }
                else
                {
                    spell.Power = newPower;
                }
            }

            // 5b. Auto-rzucanie (GAME_DESIGN.md, krok 24 tury): pozytywne zaklęcie na siebie
            //     po przeliczeniu — po rozpadzie zaklęć, żeby świeży czar nie osłabł od razu.
            //     Kosztuje turę i manę jak zwykłe rzucenie.
            await context.SaveChangesAsync();
            foreach (var kingdom in kingdoms.Where(k => k.AutoCastSpellType != null && k.TurnsAvailable > 0))
            {
                try
                {
                    var result = await battleService.CastSpellAsync(kingdom.UserId,
                        new Models.DTOs.CastSpellDto { SpellType = kingdom.AutoCastSpellType! });
                    context.KingdomEvents.Add(new KingdomEvent
                    {
                        KingdomId = kingdom.Id,
                        Category = "Magic",
                        Message = result.Success
                            ? $"Auto-rzucanie: {result.Message}"
                            : $"Auto-rzucanie nie powiodło się: {result.Message}"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd auto-rzucania dla księstwa {KingdomId}", kingdom.Id);
                }
            }

            // 5c. Smoki przychodzą teraz pasywnie co turę (DragonService.ProcessTurnArrivalAsync,
            //     wywoływane z TurnService) — model hybrydowy malejący do 200. Dzienne wabienie
            //     Portalem usunięto, by nie dublować źródła smoków.

            // 6. Badania kończą się teraz przez Punkty Nauki (ResourceService),
            //    nie przez czas — patrz docs/MECHANIKA.md §13.

            // 7. Opłaty za księstwa: nieopłacone (niedarmowe, nieimperatorskie) po 20 dniach
            //    zostają zawieszone, a po 30 dniach trwale usunięte (Kingdom.PaymentDeadlineDays/DeletionDays).
            await EnforceKingdomPaymentsAsync(context);

            await context.SaveChangesAsync();

            _logger.LogInformation("Przeliczenie zakończone pomyślnie.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przeliczenia!");
        }
    }

    /// <summary>
    /// Egzekwuje opłaty za księstwa: zawiesza nieopłacone po terminie płatności,
    /// usuwa (wraz z danymi) po 30 dniach bez opłaty. Zwolnione: darmowe (pierwsze
    /// na koncie), opłacone i imperatorskie (CoalitionRole == "Imperator").
    /// </summary>
    private async Task EnforceKingdomPaymentsAsync(ApplicationDbContext context)
    {
        var unpaid = await context.Kingdoms
            .Include(k => k.User)
            .Where(k => !k.IsFree && !k.IsPaid && k.CoalitionRole != "Imperator")
            .ToListAsync();

        foreach (var kingdom in unpaid)
        {
            if (kingdom.IsPaymentDeletable)
            {
                _logger.LogInformation(
                    "Usuwam księstwo {Name} (Id {Id}) — {Days} dni bez opłaty.",
                    kingdom.Name, kingdom.Id, kingdom.DaysSinceCreation);
                await DeleteKingdomAsync(kingdom, context);
                continue;
            }

            if (kingdom.IsPaymentOverdue && !kingdom.IsSuspended)
            {
                kingdom.IsSuspended = true;
                // Zawieszonego księstwa nie da się wybrać — jeśli było aktywne, odepnij.
                if (kingdom.User.ActiveKingdomId == kingdom.Id)
                    kingdom.User.ActiveKingdomId = null;
                context.KingdomEvents.Add(new KingdomEvent
                {
                    KingdomId = kingdom.Id,
                    Category = "Payment",
                    Message = $"Księstwo zawieszone za brak opłaty. Opłać je, inaczej po " +
                              $"{Kingdom.DeletionDays} dniach od założenia zostanie usunięte."
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Trwale usuwa księstwo. Kaskady w bazie czyszczą budynki, wojsko, profesje,
    /// badania, zaklęcia, zdarzenia, własne akcje, posty i generałów; tu ręcznie
    /// usuwamy rekordy z kluczami Restrict (raporty, wiadomości, pakty, transakcje).
    /// </summary>
    private static async Task DeleteKingdomAsync(Kingdom kingdom, ApplicationDbContext context)
    {
        int id = kingdom.Id;

        await context.QueuedActions.Where(q => q.TargetKingdomId == id).ExecuteDeleteAsync();
        await context.BattleReports
            .Where(b => b.AttackerKingdomId == id || b.DefenderKingdomId == id)
            .ExecuteDeleteAsync();
        await context.Messages
            .Where(m => m.SenderKingdomId == id || m.ReceiverKingdomId == id)
            .ExecuteDeleteAsync();
        await context.Pacts
            .Where(p => p.ProposerKingdomId == id || p.TargetKingdomId == id)
            .ExecuteDeleteAsync();
        await context.MarketTransactions
            .Where(t => t.BuyerKingdomId == id || t.SellerKingdomId == id)
            .ExecuteDeleteAsync();

        // Posty forum: najpierw cudze odpowiedzi pod postami tego księstwa
        // (ParentPostId ma Restrict), potem kaskada usunie posty autora.
        await context.ForumPosts
            .Where(f => f.ParentPost != null && f.ParentPost.AuthorKingdomId == id)
            .ExecuteDeleteAsync();

        // Wyczyść odwołania bez klucza obcego
        await context.Kingdoms
            .Where(k => k.ImperatorVoteForKingdomId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(k => k.ImperatorVoteForKingdomId, (int?)null));
        if (kingdom.User.ActiveKingdomId == id)
            kingdom.User.ActiveKingdomId = null;

        context.Kingdoms.Remove(kingdom);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Losowe plagi przy przeliczeniu: Zaraza (3% — zabija 3–5% ludności; Nekromant
    /// i Olbrzym odporni), Szarańcza (3% — zjada 10–20% jedzenia), Chochliki
    /// (2% — niszczą 5–10% machin; Gnom odporny, machiny Goblina niezniszczalne).
    /// </summary>
    private static async Task ApplyRandomPlagues(Kingdom kingdom, ApplicationDbContext context)
    {
        void Notify(string message) => context.KingdomEvents.Add(new KingdomEvent
        {
            KingdomId = kingdom.Id,
            Category = "Plague",
            Message = message
        });

        // Zaraza
        if (Random.Shared.NextDouble() < 0.03
            && kingdom.Race is not ("Nekromant" or "Olbrzym") && kingdom.Population > 200)
        {
            decimal pct = 0.03m + (decimal)Random.Shared.NextDouble() * 0.02m;
            int killed = (int)(kingdom.Population * pct);
            kingdom.Population = Math.Max(100, kingdom.Population - killed);
            Notify($"Zaraza nawiedziła księstwo — zmarło {killed} mieszkańców.");
        }

        // Szarańcza
        if (Random.Shared.NextDouble() < 0.03 && kingdom.Food > 0)
        {
            decimal pct = 0.10m + (decimal)Random.Shared.NextDouble() * 0.10m;
            long eaten = (long)(kingdom.Food * pct);
            kingdom.Food -= eaten;
            Notify($"Szarańcza pożarła {eaten} jedzenia.");
        }

        // Chochliki
        if (Random.Shared.NextDouble() < 0.02
            && kingdom.Race is not ("Gnom" or "Goblin"))
        {
            var machines = await context.MilitaryUnits
                .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Machina") && m.Quantity > 0)
                .ToListAsync();
            if (machines.Count > 0)
            {
                decimal pct = 0.05m + (decimal)Random.Shared.NextDouble() * 0.05m;
                int destroyed = 0;
                foreach (var m in machines)
                {
                    int lost = Math.Max(1, (int)(m.Quantity * pct));
                    m.Quantity = Math.Max(0, m.Quantity - lost);
                    destroyed += lost;
                }
                Notify($"Chochliki zepsuły {destroyed} machin wojennych.");
            }
        }
    }

    private async Task CompleteConstruction(QueuedAction action, ApplicationDbContext context)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<ConstructionData>(action.ActionData!);
        if (data == null) return;

        var building = await context.Buildings
            .Include(b => b.Definition)
            .FirstOrDefaultAsync(b => b.KingdomId == action.KingdomId && b.BuildingType == data.BuildingType);

        if (building != null)
        {
            building.Quantity += data.Quantity;
            building.IsUnderConstruction = false;
            building.ConstructionCompletesAt = null;

            string name = building.Definition?.DisplayName ?? building.BuildingType;
            context.KingdomEvents.Add(new KingdomEvent
            {
                KingdomId = action.KingdomId,
                Category = "Construction",
                Message = $"Ukończono budowę: {name}" + (data.Quantity > 1 ? $" ×{data.Quantity}" : "")
            });
        }
    }

    /// <summary>Awansuje pct% jednostek typu fromType na jednostkę toDef (1:1, bez dodatkowych kosztów).</summary>
    private static void PromoteUnits(ApplicationDbContext context, Kingdom kingdom, string fromType, UnitDefinition toDef, decimal pct)
    {
        if (pct <= 0) return;
        var from = kingdom.MilitaryUnits.FirstOrDefault(m => m.UnitType == fromType);
        if (from == null || from.Quantity <= 0) return;

        int promote = (int)(from.Quantity * pct / 100m);
        if (promote <= 0) return;

        from.Quantity -= promote;
        var to = kingdom.MilitaryUnits.FirstOrDefault(m => m.UnitType == toDef.UnitType);
        if (to == null)
        {
            to = new MilitaryUnit { KingdomId = kingdom.Id, UnitType = toDef.UnitType, Quantity = 0, InTraining = 0 };
            context.MilitaryUnits.Add(to);
            kingdom.MilitaryUnits.Add(to);
        }
        to.Quantity += promote;

        context.KingdomEvents.Add(new KingdomEvent
        {
            KingdomId = kingdom.Id,
            Category = "Training",
            Message = $"Awans: {toDef.DisplayName} ×{promote}"
        });
    }

    private class ConstructionData
    {
        public string BuildingType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
