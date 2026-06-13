using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
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

            // 3. Odnów tury
            var kingdoms = await context.Kingdoms
                .Include(k => k.Buildings).ThenInclude(b => b.Definition)
                .Where(k => k.Era.IsActive)
                .ToListAsync();

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

                int totalTurnsToAdd = kingdom.TurnsPerDay + bonusTurns;

                // „trojtah": kumulacja maksymalnie do potrójnego dziennego przydziału
                int maxTurns = (kingdom.TurnsPerDay + bonusTurns) * 3 + 4;
                kingdom.MaxTurns = maxTurns;

                kingdom.TurnsAvailable = Math.Min(
                    kingdom.TurnsAvailable + totalTurnsToAdd,
                    maxTurns);

                kingdom.Age++;
                if (kingdom.IsProtected)
                {
                    kingdom.ProtectionDaysLeft--;
                    if (kingdom.ProtectionDaysLeft <= 0)
                        kingdom.IsProtected = false;
                }
            }

            // 4. Zakończ szkolenie jednostek
            var trainingUnits = await context.MilitaryUnits
                .Where(m => m.InTraining > 0 && m.TrainingCompletesAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var unit in trainingUnits)
            {
                unit.Quantity += unit.InTraining;
                unit.InTraining = 0;
                unit.TrainingCompletesAt = null;
            }

            // 5. Spadek siły zaklęć (oryginalny wzór):
            //    biała magia: nowa = siła·0,45 − ziemia/100
            //    czarna magia: nowa = siła·0,6 − ziemia/100
            //    (bonusy generała z Białą magią — do uzupełnienia po systemie generałów)
            var activeSpells = await context.ActiveSpells
                .Include(s => s.Spell)
                .Include(s => s.Kingdom)
                .ToListAsync();

            foreach (var spell in activeSpells)
            {
                bool isPositive = spell.Spell != null &&
                    (spell.Spell.Category == "Biała" || spell.Spell.Category == "Tarcze");
                decimal decayFactor = isPositive ? 0.45m : 0.6m;
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

            // 6. Zakończ badania
            var completedResearch = await context.Researches
                .Where(r => r.IsInProgress && r.CompletesAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var research in completedResearch)
            {
                research.IsInProgress = false;
                research.IsCompleted = true;
                research.CompletedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Przeliczenie zakończone pomyślnie.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przeliczenia!");
        }
    }

    private async Task CompleteConstruction(QueuedAction action, ApplicationDbContext context)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<ConstructionData>(action.ActionData!);
        if (data == null) return;

        var building = await context.Buildings
            .FirstOrDefaultAsync(b => b.KingdomId == action.KingdomId && b.BuildingType == data.BuildingType);

        if (building != null)
        {
            building.Quantity += data.Quantity;
            building.IsUnderConstruction = false;
            building.ConstructionCompletesAt = null;
        }
    }

    private class ConstructionData
    {
        public string BuildingType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
