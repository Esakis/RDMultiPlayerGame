using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class BattleService : IBattleService
{
    private readonly ApplicationDbContext _context;

    public BattleService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Czy rola koalicyjna pozwala działać w imieniu innych księstw koalicji.</summary>
    private static bool IsCoalitionCommander(Kingdom k) =>
        k.CoalitionRole is "Imperator" or "MainCommander";

    public async Task<ServiceResult> QueueAttackAsync(int userId, AttackDto dto)
    {
        var callerKingdom = await _context.Kingdoms
            .Include(k => k.MilitaryUnits)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (callerKingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        // Imperator/Głównodowodzący może zaplanować atak z dowolnego księstwa koalicji
        Kingdom kingdom = callerKingdom;
        if (dto.AttackerKingdomId.HasValue && dto.AttackerKingdomId != callerKingdom.Id)
        {
            if (!IsCoalitionCommander(callerKingdom))
                return ServiceResult.Fail("Tylko Imperator lub Głównodowodzący może planować ataki z innych księstw koalicji.");

            var delegated = await _context.Kingdoms
                .Include(k => k.MilitaryUnits)
                .FirstOrDefaultAsync(k => k.Id == dto.AttackerKingdomId && k.Era.IsActive);
            if (delegated == null || callerKingdom.CoalitionId == null
                || delegated.CoalitionId != callerKingdom.CoalitionId)
                return ServiceResult.Fail("Wskazane księstwo nie należy do Twojej koalicji.");
            kingdom = delegated;
        }

        if (kingdom.IsFrozen)
            return ServiceResult.Fail("Księstwo atakujące jest zamrożone — nie może atakować.");

        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Księstwo atakujące nie ma dostępnych tur.");

        var target = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);

        if (target == null)
            return ServiceResult.Fail("Nie znaleziono celu ataku.");

        if (target.Id == kingdom.Id)
            return ServiceResult.Fail("Nie możesz atakować samego siebie.");

        if (target.IsProtected)
            return ServiceResult.Fail("Cel jest pod ochroną początkową (nowicjusz).");

        if (target.IsFrozen)
            return ServiceResult.Fail("Cel jest zamrożony — nie można go zaatakować.");

        // Limit wielkości: cel nie może być 4× mniejszy ani 4× większy (docs/MECHANIKA.md §14.4)
        if (target.Land > kingdom.Land * 4 || target.Land * 4 < kingdom.Land)
            return ServiceResult.Fail("Cel jest zbyt duży lub zbyt mały (limit 4×).");

        // Koalicja: nie atakuj sojusznika; obcą koalicję tylko w stanie wojny
        if (target.CoalitionId != null)
        {
            if (target.CoalitionId == kingdom.CoalitionId)
                return ServiceResult.Fail("Nie możesz atakować członka własnej koalicji.");
            if (!await WarHelper.AreAtWarAsync(_context, kingdom.CoalitionId, target.CoalitionId))
                return ServiceResult.Fail("Możesz zaatakować członka obcej koalicji tylko w stanie wojny — wasze koalicje muszą sobie wypowiedzieć wojnę.");
        }

        // Sprawdź czy gracz ma wystarczającą ilość jednostek
        var unitsToSend = dto.Units.Where(u => u.Value > 0).ToDictionary(u => u.Key, u => u.Value);
        if (unitsToSend.Count == 0)
            return ServiceResult.Fail("Wybierz jednostki do ataku.");

        foreach (var unit in unitsToSend)
        {
            var militaryUnit = kingdom.MilitaryUnits.FirstOrDefault(m => m.UnitType == unit.Key);
            if (militaryUnit == null || militaryUnit.Quantity < unit.Value)
                return ServiceResult.Fail($"Za mało jednostek typu {unit.Key}.");
        }

        // Atak musi prowadzić generał — dostępny (w domu, zdrowy, nieuwięziony).
        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == dto.GeneralId && g.KingdomId == kingdom.Id);
        if (general == null)
            return ServiceResult.Fail("Wybierz generała, który poprowadzi atak.");
        if (general.IsPending)
            return ServiceResult.Fail("Ten generał czeka w poczekalni — najpierw go przyjmij.");
        if (general.IsImprisoned)
            return ServiceResult.Fail("Ten generał jest uwięziony.");
        if (general.IsOutside)
            return ServiceResult.Fail("Ten generał już prowadzi inny atak.");
        if (general.WoundedUntil.HasValue && general.WoundedUntil > DateTime.UtcNow)
            return ServiceResult.Fail("Ten generał jest ranny i nie może prowadzić ataku.");

        // Generał wyrusza z armią — do przeliczenia jest poza księstwem
        general.IsOutside = true;

        // Zakolejkuj atak
        var action = new QueuedAction
        {
            KingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            ActionType = "MilitaryAttack",
            ActionData = JsonSerializer.Serialize(new MilitaryAttackData { Units = unitsToSend, GeneralId = general.Id }),
            ScheduledFor = GetNextResetTime(),
            Status = "Pending"
        };

        _context.QueuedActions.Add(action);

        // Zużyj turę
        kingdom.TurnsAvailable--;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"Atak na {target.Name} pod wodzą generała {general.Name} został zaplanowany. Wykonanie podczas najbliższego przeliczenia o 5:00.");
    }

    /// <summary>Zaplanowane (jeszcze niewykonane) ataki księstwa gracza.</summary>
    public async Task<List<PlannedAttackDto>> GetPlannedAttacksAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return new List<PlannedAttackDto>();

        return await ProjectPlannedAttacksAsync(new[] { kingdom.Id });
    }

    /// <summary>
    /// Zaplanowane ataki wszystkich księstw koalicji — panel wojenny Imperatora/Głównodowodzącego.
    /// </summary>
    public async Task<List<PlannedAttackDto>> GetCoalitionPlannedAttacksAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom?.CoalitionId == null || !IsCoalitionCommander(kingdom))
            return new List<PlannedAttackDto>();

        var memberIds = await _context.Kingdoms
            .Where(k => k.CoalitionId == kingdom.CoalitionId)
            .Select(k => k.Id)
            .ToListAsync();

        return await ProjectPlannedAttacksAsync(memberIds);
    }

    private async Task<List<PlannedAttackDto>> ProjectPlannedAttacksAsync(IReadOnlyCollection<int> kingdomIds)
    {
        var actions = await _context.QueuedActions
            .Include(a => a.Kingdom)
            .Include(a => a.TargetKingdom)
            .Where(a => kingdomIds.Contains(a.KingdomId) && a.ActionType == "MilitaryAttack" && a.Status == "Pending")
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var generalIds = new List<int>();
        var parsed = new List<(QueuedAction action, MilitaryAttackData data)>();
        foreach (var a in actions)
        {
            var data = a.ActionData == null ? null : JsonSerializer.Deserialize<MilitaryAttackData>(a.ActionData);
            if (data == null) continue;
            parsed.Add((a, data));
            if (data.GeneralId > 0) generalIds.Add(data.GeneralId);
        }

        var generalNames = await _context.Generals
            .Where(g => generalIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Name);

        return parsed.Select(p => new PlannedAttackDto
        {
            Id = p.action.Id,
            AttackerKingdomId = p.action.KingdomId,
            AttackerName = p.action.Kingdom?.Name ?? "?",
            TargetKingdomId = p.action.TargetKingdomId ?? 0,
            TargetName = p.action.TargetKingdom?.Name ?? "?",
            GeneralId = p.data.GeneralId,
            GeneralName = generalNames.GetValueOrDefault(p.data.GeneralId, "—"),
            Units = p.data.Units,
            ScheduledFor = p.action.ScheduledFor,
            CreatedAt = p.action.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Dostępni generałowie i jednostki księstwa do zaplanowania ataku.
    /// Własne księstwo — zawsze; cudze — tylko Imperator/Głównodowodzący tej samej koalicji.
    /// </summary>
    public async Task<ServiceResult<AttackOptionsDto>> GetAttackOptionsAsync(int userId, int kingdomId)
    {
        var callerKingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (callerKingdom == null)
            return ServiceResult<AttackOptionsDto>.Fail("Nie znaleziono księstwa.");

        if (kingdomId != callerKingdom.Id
            && (!IsCoalitionCommander(callerKingdom) || callerKingdom.CoalitionId == null))
            return ServiceResult<AttackOptionsDto>.Fail("Brak uprawnień do tego księstwa.");

        var kingdom = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .FirstOrDefaultAsync(k => k.Id == kingdomId && k.Era.IsActive);
        if (kingdom == null || (kingdom.Id != callerKingdom.Id && kingdom.CoalitionId != callerKingdom.CoalitionId))
            return ServiceResult<AttackOptionsDto>.Fail("Wskazane księstwo nie należy do Twojej koalicji.");

        var now = DateTime.UtcNow;
        var generals = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id && !g.IsPending && !g.IsImprisoned && !g.IsOutside
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .OrderByDescending(g => g.Experience)
            .ToListAsync();

        return ServiceResult<AttackOptionsDto>.Ok(new AttackOptionsDto
        {
            KingdomId = kingdom.Id,
            KingdomName = kingdom.Name,
            TurnsAvailable = kingdom.TurnsAvailable,
            Generals = generals.Select(g => new GeneralDto
            {
                Id = g.Id,
                Name = g.Name,
                PrimaryTrait = g.PrimaryTrait,
                SecondaryTrait = g.SecondaryTrait,
                Level = g.Level,
                Experience = g.Experience,
                IsAvailable = true,
                Status = "W domu"
            }).ToList(),
            Units = kingdom.MilitaryUnits
                .Where(u => u.Quantity > 0 && !u.UnitType.EndsWith("_Zlodziej"))
                .Select(u => new AttackUnitDto
                {
                    UnitType = u.UnitType,
                    DisplayName = u.Definition?.DisplayName ?? u.UnitType,
                    Quantity = u.Quantity,
                    AttackPower = u.Definition?.AttackPower ?? 0
                }).ToList()
        });
    }

    /// <summary>
    /// Odwołuje zaplanowany atak przed przeliczeniem: zwraca turę i generała do domu.
    /// Własne ataki — zawsze; ataki księstw koalicji — Imperator/Głównodowodzący.
    /// </summary>
    public async Task<ServiceResult> CancelPlannedAttackAsync(int userId, int actionId)
    {
        var callerKingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (callerKingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var action = await _context.QueuedActions
            .Include(a => a.Kingdom)
            .FirstOrDefaultAsync(a => a.Id == actionId && a.ActionType == "MilitaryAttack");
        if (action == null)
            return ServiceResult.Fail("Nie znaleziono zaplanowanego ataku.");

        bool ownAttack = action.KingdomId == callerKingdom.Id;
        bool commanderOfCoalition = IsCoalitionCommander(callerKingdom)
            && callerKingdom.CoalitionId != null
            && action.Kingdom.CoalitionId == callerKingdom.CoalitionId;
        if (!ownAttack && !commanderOfCoalition)
            return ServiceResult.Fail("Nie masz uprawnień do odwołania tego ataku.");

        if (action.Status != "Pending")
            return ServiceResult.Fail("Tego ataku nie można już odwołać — został wykonany lub anulowany.");

        action.Status = "Cancelled";

        // Generał wraca do domu
        var data = action.ActionData == null ? null : JsonSerializer.Deserialize<MilitaryAttackData>(action.ActionData);
        if (data != null && data.GeneralId > 0)
        {
            var general = await _context.Generals
                .FirstOrDefaultAsync(g => g.Id == data.GeneralId && g.KingdomId == action.KingdomId);
            if (general != null) general.IsOutside = false;
        }

        // Zwrot zużytej tury księstwu, z którego atak miał wyruszyć
        action.Kingdom.TurnsAvailable = Math.Min(action.Kingdom.MaxTurns, action.Kingdom.TurnsAvailable + 1);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Atak został odwołany. Tura wróciła do puli, a generał do księstwa.");
    }

    public async Task<List<BattleReportDto>> GetBattleReportsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null) return new List<BattleReportDto>();

        return await ProjectReportsAsync(b =>
            b.AttackerKingdomId == kingdom.Id || b.DefenderKingdomId == kingdom.Id);
    }

    /// <summary>
    /// Raporty koalicyjne: wszystkie bitwy, w których uczestniczyło (atakując lub broniąc się)
    /// dowolne księstwo koalicji gracza.
    /// </summary>
    public async Task<List<BattleReportDto>> GetCoalitionBattleReportsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom?.CoalitionId == null) return new List<BattleReportDto>();

        var memberIds = await _context.Kingdoms
            .Where(k => k.CoalitionId == kingdom.CoalitionId)
            .Select(k => k.Id)
            .ToListAsync();

        return await ProjectReportsAsync(b =>
            memberIds.Contains(b.AttackerKingdomId) || memberIds.Contains(b.DefenderKingdomId));
    }

    private async Task<List<BattleReportDto>> ProjectReportsAsync(
        System.Linq.Expressions.Expression<Func<BattleReport, bool>> filter)
    {
        return await _context.BattleReports
            .Where(filter)
            .Include(b => b.AttackerKingdom)
            .Include(b => b.DefenderKingdom)
            .OrderByDescending(b => b.OccurredAt)
            .Take(100)
            .Select(b => new BattleReportDto
            {
                Id = b.Id,
                AttackerKingdomId = b.AttackerKingdomId,
                DefenderKingdomId = b.DefenderKingdomId,
                AttackerName = b.AttackerKingdom.Name,
                DefenderName = b.DefenderKingdom.Name,
                BattleType = b.BattleType,
                Result = b.Result,
                AttackerLosses = b.AttackerLosses,
                DefenderLosses = b.DefenderLosses,
                ResourcesStolen = b.ResourcesStolen,
                LandCaptured = b.LandCaptured,
                OccurredAt = b.OccurredAt
            })
            .ToListAsync();
    }

    public async Task<BattleResult> ExecuteMilitaryAttackAsync(QueuedAction action)
    {
        var attackData = JsonSerializer.Deserialize<MilitaryAttackData>(action.ActionData!);
        if (attackData == null)
            return new BattleResult { Success = false };

        var attacker = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .FirstOrDefaultAsync(k => k.Id == action.KingdomId);

        var defender = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .Include(k => k.Professions)
            .Include(k => k.ActiveSpells)
            .FirstOrDefaultAsync(k => k.Id == action.TargetKingdomId);

        if (attacker == null || defender == null)
            return new BattleResult { Success = false };

        // Zamrożony obrońca jest chroniony — atak nie dochodzi do skutku
        if (defender.IsFrozen)
            return new BattleResult { Success = false };

        var attackerRace = await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == attacker.Race) ?? new RaceDefinition { Name = attacker.Race };
        var defenderRace = await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == defender.Race) ?? new RaceDefinition { Name = defender.Race };

        // Generałowie (potrzebni już przed obliczeniem obrony — Smokobójstwo zabija smoki obrońcy).
        var now = DateTime.UtcNow;
        var attackerGenerals = await _context.Generals
            .Where(g => g.KingdomId == attacker.Id && !g.IsImprisoned && !g.IsPending
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .ToListAsync();
        var defenderGenerals = await _context.Generals
            .Where(g => g.KingdomId == defender.Id && !g.IsImprisoned && !g.IsOutside && !g.IsPending
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .ToListAsync();

        // Najwyższy poziom generała o danej cesze drugorzędnej (0 = brak).
        int SecLevel(IEnumerable<General> gens, string trait) =>
            gens.Where(g => g.SecondaryTrait == trait).Select(g => g.Level).DefaultIfEmpty(0).Max();

        // Atak prowadzi generał wskazany przy planowaniu. Jeśli w międzyczasie wypadł z gry
        // (porwany, zabity, uwięziony, ranny) — armia zawraca i atak nie dochodzi do skutku.
        General? leadingGeneral;
        if (attackData.GeneralId > 0)
        {
            leadingGeneral = attackerGenerals.FirstOrDefault(g => g.Id == attackData.GeneralId);
            if (leadingGeneral == null)
            {
                _context.KingdomEvents.Add(new KingdomEvent
                {
                    KingdomId = attacker.Id,
                    Category = "Battle",
                    Message = $"Atak na {defender.Name} nie doszedł do skutku — generał prowadzący wypadł z gry (porwany, uwięziony lub ranny)."
                });
                await _context.SaveChangesAsync();
                return new BattleResult { Success = false };
            }
        }
        else
        {
            // Stare zlecenia bez generała: dobierz najlepszego Wodza w domu (zgodność wsteczna)
            leadingGeneral = attackerGenerals
                .Where(g => g.PrimaryTrait == "Wodz" && !g.IsOutside)
                .OrderByDescending(g => g.Experience)
                .FirstOrDefault();
        }

        // Smokobójstwo (atakujący, Dracopedia §11): zabija lvl% smoków obrońcy oraz
        // z szansą lvl/4% burzy wabiki smoków (Portal, Smokodrap) — przed wyliczeniem obrony.
        int dragonSlayLvl = SecLevel(attackerGenerals, "Smokobojstwo");
        if (dragonSlayLvl > 0)
        {
            decimal killPct = Math.Min(1m, dragonSlayLvl / 100m);
            foreach (var d in defender.MilitaryUnits.Where(u => u.UnitType.EndsWith("_Smok") && u.Quantity > 0))
                d.Quantity = Math.Max(0, d.Quantity - (int)Math.Ceiling(d.Quantity * killPct));
            foreach (var bt in new[] { "Portal", "Smokodrap" })
            {
                var b = defender.Buildings.FirstOrDefault(x => x.BuildingType == bt && x.Quantity > 0 && !x.IsUnderConstruction);
                if (b != null && Random.Shared.NextDouble() < dragonSlayLvl / 400.0)
                    b.Quantity = Math.Max(0, b.Quantity - 1);
            }
        }

        // Wieże obrońcy kontra machiny (manual „armada"): każda wieża niszczy 3 machiny
        // i blokuje 12 (wieże Br-Ouga: 2 i 8); machiny Goblina są niezniszczalne (§2.2).
        var machineKeys = attackData.Units
            .Where(u => u.Value > 0 && u.Key.EndsWith("_Machina"))
            .Select(u => u.Key).ToList();
        if (machineKeys.Count > 0)
        {
            long defTowers = defender.Buildings
                .Where(b => b.BuildingType == "WiezeObronne" && !b.IsUnderConstruction)
                .Sum(b => (long)b.Quantity);
            if (defTowers > 0)
            {
                long sentMachines = machineKeys.Sum(k => (long)attackData.Units[k]);
                int destroyPerTower = defender.Race == "Br-Oug" ? 2 : 3;
                int blockPerTower = defender.Race == "Br-Oug" ? 8 : 12;
                long destroyed = attacker.Race == "Goblin"
                    ? 0 : Math.Min(sentMachines, defTowers * destroyPerTower);
                long blocked = Math.Min(sentMachines, Math.Max(defTowers * blockPerTower, destroyed));

                long toDestroy = destroyed;
                foreach (var key in machineKeys)
                {
                    if (toDestroy <= 0) break;
                    var unit = attacker.MilitaryUnits.FirstOrDefault(m => m.UnitType == key);
                    if (unit == null) continue;
                    int hit = (int)Math.Min(unit.Quantity, toDestroy);
                    unit.Quantity -= hit;
                    toDestroy -= hit;
                }

                // zablokowane (w tym zniszczone) machiny nie uczestniczą w bitwie
                long toExclude = blocked;
                foreach (var key in machineKeys)
                {
                    if (toExclude <= 0) break;
                    int cut = (int)Math.Min(attackData.Units[key], toExclude);
                    attackData.Units[key] -= cut;
                    toExclude -= cut;
                }

                if (destroyed > 0)
                    _context.KingdomEvents.Add(new KingdomEvent
                    {
                        KingdomId = attacker.Id,
                        Category = "Battle",
                        Message = $"Wieże obronne {defender.Name} zniszczyły {destroyed} machin i zablokowały {blocked}."
                    });
            }
        }

        // Siły wg oryginalnych wzorów (docs/MECHANIKA.md §8)
        long attackPower = BattleCalculator.CalculateAttackPower(attacker, attackData.Units, attackerRace);
        long defensePower = BattleCalculator.CalculateDefensePower(defender, defenderRace);

        // Badania broni (Ostrzenie/Naprawa/Przekuwanie) zwiększają siłę ataku
        decimal atkBonus = await ResearchEffects.MaxEffectAsync(_context, attacker.Id, "AttackBonus");
        if (atkBonus > 0) attackPower = (long)(attackPower * (1m + atkBonus));

        // Gniew Enta: atakujący Ent w szale ma +100% siły ataku (szał zużywa się przy ataku)
        if (attacker.Race == "Ent" && attacker.EntWrathActive)
        {
            attackPower *= 2;
            attacker.EntWrathActive = false;
        }

        // Nauka stosowana Człowieka: szkoła wojskowa +10% ataku
        if (attacker.Race == "Człowiek" && attacker.AppliedScienceSchool == "Military")
            attackPower = (long)(attackPower * 1.10m);

        // Generałowie: prowadzący Wódz zwiększa atak o lvl% (inny generał prowadzi bez bonusu),
        // Obrońca (najlepszy w domu) zwiększa obronę o lvl%
        if (leadingGeneral != null)
        {
            if (leadingGeneral.PrimaryTrait == "Wodz")
                attackPower = (long)(attackPower * (1.0 + leadingGeneral.Level / 100.0));
            leadingGeneral.IsOutside = true; // prowadzi atak — wraca po przeliczeniu
        }

        var bestDefenderGeneral = defenderGenerals
            .Where(g => g.PrimaryTrait == "Obronca")
            .OrderByDescending(g => g.Experience)
            .FirstOrDefault();
        if (bestDefenderGeneral != null)
            defensePower = (long)(defensePower * (1.0 + bestDefenderGeneral.Level / 100.0));

        // Pakty wojskowe: armia partnera w domu broni (bez wież i domobrany),
        // skuteczność 50%/45%/40% wg liczby paktów
        var militaryPartners = await PactService.GetActivePactPartnersAsync(_context, defender.Id, "Wojskowy");
        if (militaryPartners.Count > 0)
        {
            decimal efficiency = PactService.PactEfficiency(militaryPartners.Count);
            foreach (var partner in militaryPartners)
            {
                long partnerDefense = partner.MilitaryUnits
                    .Where(u => u.Definition != null && u.Quantity > 0
                                && !u.UnitType.EndsWith("_Zlodziej") && !u.UnitType.EndsWith("_Machina"))
                    .Sum(u => (long)u.Quantity * u.Definition!.DefensePower);
                defensePower += (long)(partnerDefense * efficiency);
            }
        }

        // Losowość ±5%
        double randomFactor = BattleCalculator.GetRandomFactor();
        attackPower = (long)(attackPower * randomFactor);

        // Nawiedzony las (Dracopedia §14.3): 5% armii inwazyjnej ucieka przed walką
        if (HasBld(defender, "NawiedzonyLas"))
            attackPower = (long)(attackPower * 0.95m);

        // Straszny lasek Elfa (cecha rasowa, MECHANIKA §2.2): odstrasza 10% armii inwazyjnej
        if (defender.Race == "Elf")
            attackPower = (long)(attackPower * 0.90m);

        bool attackerWins = attackPower > defensePower;

        // Straty (~15% przy równowadze; modyfikatory rasowe: Krasnolud −25%, Ent −50%)
        var attackerCasualties = BattleCalculator.CalculateCasualties(
            attackData.Units, attackPower, defensePower, attackerWins, attackerRace);
        var defenderCasualties = BattleCalculator.CalculateDefenderCasualties(
            defender.MilitaryUnits, attackPower, defensePower, attackerWins, defenderRace);

        // Ambulatorium polowe (Dracopedia §14.3): mobilny lazaret — straty własne w ataku −50%
        if (HasBld(attacker, "AmbulatoriumPolowe"))
            foreach (var key in attackerCasualties.Keys.ToList())
                attackerCasualties[key] /= 2;

        // Uzdrawianie (Dracopedia §11): ratuje lvl% poległych własnych —
        // atakujący ×2 po wygranej, obrońca ×4 po udanej obronie.
        int atkHealLvl = SecLevel(attackerGenerals, "Uzdrawianie");
        if (atkHealLvl > 0)
        {
            decimal save = Math.Min(0.95m, atkHealLvl / 100m * (attackerWins ? 2m : 1m));
            foreach (var key in attackerCasualties.Keys.ToList())
                attackerCasualties[key] = (int)(attackerCasualties[key] * (1m - save));
        }
        int defHealLvl = SecLevel(defenderGenerals, "Uzdrawianie");
        if (defHealLvl > 0)
        {
            decimal save = Math.Min(0.95m, defHealLvl / 100m * (!attackerWins ? 4m : 1m));
            foreach (var key in defenderCasualties.Keys.ToList())
                defenderCasualties[key] = (int)(defenderCasualties[key] * (1m - save));
        }

        // Krwiożerczość (atakujący, Dracopedia §11): +2·lvl% strat obrońcy po wygranej.
        int bloodLvl = SecLevel(attackerGenerals, "Krwiozerczonsc");
        if (bloodLvl > 0 && attackerWins)
        {
            decimal extra = Math.Min(1m, 2m * bloodLvl / 100m);
            foreach (var unit in defender.MilitaryUnits)
            {
                if (unit.Quantity <= 0 || unit.UnitType.EndsWith("_Smok") || unit.UnitType.EndsWith("_Zlodziej")) continue;
                int already = defenderCasualties.TryGetValue(unit.UnitType, out var v) ? v : 0;
                int more = (int)((unit.Quantity - already) * extra);
                if (more > 0) defenderCasualties[unit.UnitType] = already + more;
            }
        }

        // Zastosuj straty atakującego
        foreach (var casualty in attackerCasualties)
        {
            var unit = attacker.MilitaryUnits.FirstOrDefault(m => m.UnitType == casualty.Key);
            if (unit != null)
                unit.Quantity = Math.Max(0, unit.Quantity - casualty.Value);
        }

        // Zastosuj straty obrońcy
        foreach (var casualty in defenderCasualties)
        {
            var unit = defender.MilitaryUnits.FirstOrDefault(m => m.UnitType == casualty.Key);
            if (unit != null)
                unit.Quantity = Math.Max(0, unit.Quantity - casualty.Value);
        }

        // Nekromancja: polegli żołnierze zasilają cmentarz Nekromanta
        long attackerDead = attackerCasualties.Sum(c => (long)c.Value);
        long defenderDead = defenderCasualties.Sum(c => (long)c.Value);
        if (defender.Race == "Nekromant") defender.Bodies += attackerDead + defenderDead;
        if (attacker.Race == "Nekromant") attacker.Bodies += attackerDead;

        // Renowacja broni (Dracopedia §14.3): odzyskuje 5 broni za każdego własnego poległego
        bool HasBld(Kingdom k2, string type) => k2.Buildings.Any(b =>
            b.BuildingType == type && b.Quantity > 0 && !b.IsUnderConstruction);
        if (attackerDead > 0 && HasBld(attacker, "RenowacjaBroni"))
            attacker.Weapons += attackerDead * 5;
        if (defenderDead > 0 && HasBld(defender, "RenowacjaBroni"))
            defender.Weapons += defenderDead * 5;

        // Gniew Enta: Ent, który poniósł straty, wpada w szał (+100% ataku do przeliczenia)
        if (defender.Race == "Ent" && defenderDead > 0) defender.EntWrathActive = true;

        // Straty cywilów obrońcy (25% strat armii; 50%/100% przy domobranie)
        double defCasualtyRate = defenderCasualties.Count > 0 && defender.MilitaryUnits.Sum(u => u.Quantity) > 0
            ? (double)defenderCasualties.Sum(c => c.Value) / Math.Max(1, defender.MilitaryUnits.Sum(u => u.Quantity))
            : 0.0;
        int civilianLosses = BattleCalculator.CalculateCivilianLosses(defender, defCasualtyRate, !attackerWins);
        defender.Population = Math.Max(100, defender.Population - civilianLosses);

        int landCaptured = 0;
        ResourcesStolen? resourcesStolen = null;

        if (attackerWins)
        {
            // pierwszy przechodzący atak: ~11% obszaru (Hobbit 9%, Sieć fortec −2 p.p.)
            landCaptured = BattleCalculator.CalculateLandCaptured(defender, defenderRace);
            resourcesStolen = BattleCalculator.CalculateResourcesStolen(defender);

            // Szamanizm Olbrzyma: totemy Grabieży / Niszczycielstwa / Smokobójstwa
            if (attacker.Race == "Olbrzym")
            {
                if (attacker.TotemPlunder > 0)
                {
                    decimal m = 1m + 0.05m * attacker.TotemPlunder;
                    resourcesStolen.Gold = (long)(resourcesStolen.Gold * m);
                    resourcesStolen.Food = (long)(resourcesStolen.Food * m);
                    resourcesStolen.Stone = (long)(resourcesStolen.Stone * m);
                    resourcesStolen.Weapons = (long)(resourcesStolen.Weapons * m);
                }
                if (attacker.TotemDestruction > 0)
                    landCaptured = (int)(landCaptured * (1m + 0.05m * attacker.TotemDestruction));
                if (attacker.TotemDragonSlay > 0)
                {
                    decimal killPct = Math.Min(1m, 0.05m * attacker.TotemDragonSlay);
                    foreach (var d in defender.MilitaryUnits.Where(u => u.UnitType.EndsWith("_Smok") && u.Quantity > 0))
                        d.Quantity = Math.Max(0, d.Quantity - (int)(d.Quantity * killPct));
                }
            }

            defender.Land -= landCaptured;
            attacker.Land += landCaptured;

            attacker.Gold += resourcesStolen.Gold;
            defender.Gold -= resourcesStolen.Gold;
            attacker.Food += resourcesStolen.Food;
            defender.Food -= resourcesStolen.Food;
            attacker.Stone += resourcesStolen.Stone;
            defender.Stone -= resourcesStolen.Stone;
            attacker.Weapons += resourcesStolen.Weapons;
            defender.Weapons -= resourcesStolen.Weapons;

            // Udany atak na członka koalicji resetuje budowę Pałacu Sądu Ostatecznego
            if (defender.CoalitionId != null)
            {
                var defCoalition = await _context.Coalitions
                    .FirstOrDefaultAsync(c => c.Id == defender.CoalitionId && c.IsBuildingPps);
                if (defCoalition != null)
                {
                    defCoalition.PpsBudulec = 0;
                    defCoalition.PSOProgress = 0m;
                }
            }
        }

        // Sabotaż (atakujący, Dracopedia §11): po wygranej burzy lvl/2% zwykłych budynków
        // obrońcy oraz z szansą 2·lvl% jeden budynek specjalny.
        int sabotageLvl = SecLevel(attackerGenerals, "Sabotaz");
        if (sabotageLvl > 0 && attackerWins)
        {
            decimal demolishPct = sabotageLvl / 200m;
            foreach (var b in defender.Buildings.Where(x => x.Definition != null && !x.Definition.IsSpecial && x.Quantity > 0 && !x.IsUnderConstruction))
                b.Quantity = Math.Max(0, b.Quantity - (int)Math.Ceiling(b.Quantity * demolishPct));
            if (Random.Shared.NextDouble() < Math.Min(0.9, 2.0 * sabotageLvl / 100.0))
            {
                var special = defender.Buildings.FirstOrDefault(x => x.Definition != null && x.Definition.IsSpecial && x.Quantity > 0 && !x.IsUnderConstruction);
                if (special != null) special.Quantity = 0;
            }
        }

        // Machiny wojenne (manual „armada", MECHANIKA §2.2): wysłane z hoplitami burzą
        // infrastrukturę — pełny efekt (20% infrastruktury i 5 budynków specjalnych)
        // przy 2,5 machiny na akr obrońcy (Br-Oug burzy o 40% słabiej); wysłane z E1
        // tylko zdobywają ziemię. Machiny Goblina z E2 obniżają obronę celu o 20%
        // swojej siły przy kolejnych atakach w tym przeliczeniu.
        long activeMachines = attackData.Units
            .Where(u => u.Value > 0 && u.Key.EndsWith("_Machina")).Sum(u => (long)u.Value);
        if (activeMachines > 0)
        {
            bool UnitKindSent(Func<Models.Entities.UnitDefinition, bool> pred) =>
                attackData.Units.Any(u => u.Value > 0 && attacker.MilitaryUnits.Any(m =>
                    m.UnitType == u.Key && m.Definition != null && pred(m.Definition)));
            bool withHoplites = attackData.Units.Any(u => u.Value > 0 && u.Key.EndsWith("_Hoplita"));
            bool withE1 = UnitKindSent(d => d.RequiredBuilding == "OltarzInicjacji");
            bool withE2 = UnitKindSent(d => d.RequiredBuilding == "KoszarySpecjalne");

            if (attackerWins && withHoplites && !withE1)
            {
                decimal scale = Math.Min(1m, activeMachines / (2.5m * Math.Max(1, defender.Land)));
                if (attacker.Race == "Br-Oug") scale *= 0.6m;
                foreach (var b in defender.Buildings.Where(x => x.Definition != null
                    && !x.Definition.IsSpecial && x.Quantity > 0 && !x.IsUnderConstruction))
                    b.Quantity = Math.Max(0, b.Quantity - (int)(b.Quantity * 0.20m * scale));
                foreach (var special in defender.Buildings
                    .Where(x => x.Definition != null && x.Definition.IsSpecial
                        && x.Quantity > 0 && !x.IsUnderConstruction)
                    .OrderBy(_ => Random.Shared.Next()).Take((int)(5m * scale)))
                    special.Quantity = 0;
            }

            if (attacker.Race == "Goblin" && withE2)
                defender.SiegeDefensePenalty += (long)(activeMachines * attackerRace.MachineAttack * 0.20m);
        }

        // Rabunek (atakujący, Dracopedia §11): po wygranej niszczy 2·lvl% zapasów obrońcy
        // (połowę zagarnia) oraz lvl/2% jego infrapunktów.
        int plunderLvl = SecLevel(attackerGenerals, "Rabunek");
        if (plunderLvl > 0 && attackerWins)
        {
            decimal pct = Math.Min(0.9m, 2m * plunderLvl / 100m);
            long g = (long)(defender.Gold * pct); defender.Gold -= g; attacker.Gold += g / 2;
            long f = (long)(defender.Food * pct); defender.Food -= f; attacker.Food += f / 2;
            long st = (long)(defender.Stone * pct); defender.Stone -= st; attacker.Stone += st / 2;
            long w = (long)(defender.Weapons * pct); defender.Weapons -= w; attacker.Weapons += w / 2;
            long inf = (long)(defender.BudulecStored * (plunderLvl / 200m));
            defender.BudulecStored = Math.Max(0, defender.BudulecStored - inf);
        }

        // Czarna magia (atakujący, Dracopedia §11): może zdjąć białą magię/tarcze celu (1,5·lvl%)
        // oraz wywołać Smoczy oddech przy ataku (2·lvl%).
        int blackLvl = SecLevel(attackerGenerals, "CzarnaMagia");
        if (blackLvl > 0)
        {
            if (defender.ActiveSpells.Count > 0 && Random.Shared.NextDouble() < Math.Min(0.9, 1.5 * blackLvl / 100.0))
            {
                var positive = defender.ActiveSpells.FirstOrDefault(s =>
                    s.SpellType is "TarczaWojenna" or "DobryHumor" or "Pracowitosc" or "FluidMagiczny"
                        or "Plodnosc" or "Szczescie" or "PadleLegiony" or "TarczaAntymagiczna" or "ZwierciadloMagiczne");
                if (positive != null) _context.ActiveSpells.Remove(positive);
            }
            if (Random.Shared.NextDouble() < Math.Min(0.9, 2.0 * blackLvl / 100.0))
            {
                int popKilled = (int)(defender.Population * 0.05);
                defender.Population = Math.Max(100, defender.Population - popKilled);
            }
        }

        // Magia czasu (atakujący, Dracopedia §11): 2·lvl% szansy na kradzież 1–4 tur obrońcy.
        int timeLvl = SecLevel(attackerGenerals, "MagiaCzasu");
        if (timeLvl > 0 && defender.TurnsAvailable > 0
            && Random.Shared.NextDouble() < Math.Min(0.9, 2.0 * timeLvl / 100.0))
        {
            int stolen = Math.Min(defender.TurnsAvailable, Random.Shared.Next(1, 5));
            defender.TurnsAvailable -= stolen;
            attacker.TurnsAvailable = Math.Min(attacker.MaxTurns, attacker.TurnsAvailable + stolen);
        }

        // Cechy przeciw generałom w bitwie (Dracopedia §11): Porwanie 2·lvl% i Zabójstwo
        // 2·lvl% działają po zwycięstwie; Zranienie 3·lvl% (na 3 dni) także przy porażce.
        General? RandomDefenderGeneral() => defenderGenerals
            .Where(g => !g.IsImprisoned)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();
        void NotifyBoth(string message)
        {
            _context.KingdomEvents.Add(new KingdomEvent { KingdomId = attacker.Id, Category = "Battle", Message = message });
            _context.KingdomEvents.Add(new KingdomEvent { KingdomId = defender.Id, Category = "Battle", Message = message });
        }

        int kidnapLvl = SecLevel(attackerGenerals, "PorwanieGenerala");
        if (kidnapLvl > 0 && attackerWins
            && Random.Shared.NextDouble() < Math.Min(0.9, 2.0 * kidnapLvl / 100.0))
        {
            var victim = RandomDefenderGeneral();
            if (victim != null)
            {
                victim.IsImprisoned = true;
                NotifyBoth($"Bitwa {attacker.Name} ⚔ {defender.Name}: porwano generała {victim.Name} (poziom {victim.Level}) — trafił do lochów.");
            }
        }

        int killGenLvl = SecLevel(attackerGenerals, "ZabojstwoGenerala");
        if (killGenLvl > 0 && attackerWins
            && Random.Shared.NextDouble() < Math.Min(0.9, 2.0 * killGenLvl / 100.0))
        {
            var victim = RandomDefenderGeneral();
            if (victim != null)
            {
                defenderGenerals.Remove(victim);
                _context.Generals.Remove(victim);
                NotifyBoth($"Bitwa {attacker.Name} ⚔ {defender.Name}: generał {victim.Name} (poziom {victim.Level}) poległ z ręki skrytobójcy.");
            }
        }

        int woundLvl = SecLevel(attackerGenerals, "ZranienieGenerala");
        if (woundLvl > 0 && Random.Shared.NextDouble() < Math.Min(0.9, 3.0 * woundLvl / 100.0))
        {
            var victim = RandomDefenderGeneral();
            if (victim != null)
            {
                // Pałac (Dracopedia §14.3): ranni generałowie wracają do sił 2× szybciej
                bool defenderPalac = HasBld(defender, "Palac");
                victim.WoundedUntil = DateTime.UtcNow.AddDays(defenderPalac ? 1.5 : 3);
                NotifyBoth($"Bitwa {attacker.Name} ⚔ {defender.Name}: generał {victim.Name} został ranny (wraca do sił za {(defenderPalac ? "półtora dnia" : "3 dni")}).");
            }
        }

        // Doświadczenie generałów: zależne od sumy sił i wyrównania starcia
        long expGain = (long)((attackPower + defensePower) / 1000.0
            * Math.Min(1.0, (double)Math.Min(attackPower, defensePower) / Math.Max(1, Math.Max(attackPower, defensePower))));
        if (attackerWins && leadingGeneral != null)
            leadingGeneral.Experience += Math.Max(10, expGain + landCaptured * 5L);
        else if (!attackerWins && bestDefenderGeneral != null)
            bestDefenderGeneral.Experience += Math.Max(10, expGain);

        // Raport
        var report = new BattleReport
        {
            AttackerKingdomId = attacker.Id,
            DefenderKingdomId = defender.Id,
            BattleType = "Military",
            Result = attackerWins ? "Victory" : "Defeat",
            AttackerLosses = JsonSerializer.Serialize(attackerCasualties),
            DefenderLosses = JsonSerializer.Serialize(defenderCasualties),
            ResourcesStolen = resourcesStolen != null ? JsonSerializer.Serialize(resourcesStolen) : null,
            LandCaptured = landCaptured,
            OccurredAt = DateTime.UtcNow
        };

        _context.BattleReports.Add(report);
        await _context.SaveChangesAsync();

        return new BattleResult
        {
            Success = true,
            AttackerWins = attackerWins,
            LandCaptured = landCaptured,
            ResourcesStolen = resourcesStolen
        };
    }

    // ====================== MAGIA ======================

    /// <summary>
    /// Koszt zaklęcia wg oryginału: ckp · (1 + ziemia/2000) · (1 − czarodziejstwo/100),
    /// drożenie za każde zaklęcie dnia: 10% (z Pałacem magicznym 8%; Elf 9%, Dżin 8%/6%).
    /// </summary>
    private async Task<long> CalculateSpellCostAsync(SpellDefinition spell, Kingdom kingdom, int castsToday)
    {
        decimal sorceryDiscount = await _context.Researches
            .Where(r => r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Czarodziejstwo"))
            .Select(r => (decimal?)r.Tech!.EffectValue)
            .MaxAsync() ?? 0m;

        bool hasMagicPalace = kingdom.Buildings.Any(b =>
            b.BuildingType == "PalacMagiczny" && b.Quantity > 0 && !b.IsUnderConstruction);

        // PL manual: drożenie +10% za zaklęcie; Dżin z Pałacem magicznym 9%, Gnom 11%
        decimal growth = kingdom.Race switch
        {
            "Dżin" => hasMagicPalace ? 1.09m : 1.10m,
            "Gnom" => 1.11m,
            _ => 1.10m
        };

        decimal cost = spell.ManaCost
            * (1m + kingdom.Land / 2000m)
            * (1m - sorceryDiscount);

        for (int i = 0; i < castsToday; i++)
            cost *= growth;

        // Biała magia dla Elfa o 25% tańsza
        if (kingdom.Race == "Elf" && spell.Category == "Biała")
            cost *= 0.75m;

        // Metamagia Dżina: wzmocniona +25% ceny, przyspieszona −10% ceny
        if (kingdom.Race == "Dżin")
            cost *= kingdom.MetamagicMode switch
            {
                "Strengthened" => 1.25m,
                "Accelerated" => 0.90m,
                _ => 1m
            };

        // Przywołanie smoka: koszt rośnie stromo z liczbą posiadanych smoków
        if (spell.EffectType == "SummonDragon")
        {
            long dragons = await _context.MilitaryUnits
                .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Smok"))
                .SumAsync(m => (long)m.Quantity);
            cost *= DragonHelper.SummonCostMultiplier(dragons);
        }

        return Math.Max(1, (long)cost);
    }

    /// <summary>Liczba posiadanych smoków i aktualny limit (budynki + badania).</summary>
    private async Task<(long dragons, long cap)> GetDragonStatusAsync(Kingdom kingdom)
    {
        long dragons = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Smok"))
            .SumAsync(m => (long)m.Quantity);
        int draco = await _context.Researches
            .CountAsync(r => r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Smoko"));
        return (dragons, DragonHelper.ComputeCap(kingdom, draco));
    }

    /// <summary>
    /// Nekromancja (docs/MECHANIKA.md §2.2): Ofiarowanie zamienia 10% populacji w ciała;
    /// przywołania wskrzeszają jednostki kosztem wolnych magów i ciał.
    /// Wolni magowie = magowie − 0,5×(armia + złodzieje).
    /// </summary>
    private async Task<ServiceResult> HandleNecromancyAsync(Kingdom kingdom, SpellDefinition spell, int mages)
    {
        if (spell.EffectType == "Sacrifice")
        {
            long sacrificed = (long)(kingdom.Population * 0.10);
            if (sacrificed <= 0) return ServiceResult.Fail("Za mała populacja na ofiarowanie.");
            kingdom.Population = Math.Max(100, kingdom.Population - (int)sacrificed);
            kingdom.Bodies += sacrificed;
            return ServiceResult.Ok($"Ofiarowano {sacrificed} mieszkańców — ciała trafiły na cmentarz (razem {kingdom.Bodies}).");
        }

        var units = await _context.MilitaryUnits.Where(m => m.KingdomId == kingdom.Id).ToListAsync();
        long soldiers = units.Where(u => !u.UnitType.EndsWith("_Zlodziej") && !u.UnitType.EndsWith("_Smok"))
            .Sum(u => (long)u.Quantity);
        long thieves = units.Where(u => u.UnitType.EndsWith("_Zlodziej")).Sum(u => (long)u.Quantity);
        double freeMages = Math.Max(0, mages - 0.5 * (soldiers + thieves));

        (string unitType, double pct, double bodyCost) = spell.EffectType switch
        {
            "SummonE2" => ("Nekromant_Ghul", 0.10, 1.0),
            "SummonE1" => ("Nekromant_Szkielet", 0.26, 1.0 / 6),
            "SummonHoplites" => ("Nekromant_Hoplita", 0.50, 1.0 / 6),
            "SummonThieves" => ("Nekromant_Zlodziej", 0.50, 0.5),
            _ => ("", 0, 0)
        };
        if (unitType == "") return ServiceResult.Fail("Nieznane przywołanie.");

        int byMages = (int)(freeMages * pct);
        int byBodies = (int)(kingdom.Bodies / bodyCost);
        int summon = Math.Min(byMages, byBodies);
        if (summon <= 0)
            return ServiceResult.Fail("Za mało wolnych magów lub ciał, by kogoś wskrzesić.");

        long bodiesUsed = (long)Math.Ceiling(summon * bodyCost);
        kingdom.Bodies = Math.Max(0, kingdom.Bodies - bodiesUsed);

        var unit = units.FirstOrDefault(u => u.UnitType == unitType);
        if (unit == null)
            _context.MilitaryUnits.Add(new MilitaryUnit
            {
                KingdomId = kingdom.Id, UnitType = unitType, Quantity = summon, InTraining = 0
            });
        else
            unit.Quantity += summon;

        return ServiceResult.Ok($"Wskrzeszono {summon}× {unitType.Replace("Nekromant_", "")} (zużyto {bodiesUsed} ciał).");
    }

    /// <summary>Dodaje 1 smoka do armii księstwa (jednostka rasowa _Smok).</summary>
    private async Task<bool> AddDragonAsync(Kingdom kingdom)
    {
        var dragonDef = await _context.UnitDefinitions
            .FirstOrDefaultAsync(u => u.Race == kingdom.Race && u.UnitType.EndsWith("_Smok"));
        if (dragonDef == null) return false;

        var unit = await _context.MilitaryUnits
            .FirstOrDefaultAsync(m => m.KingdomId == kingdom.Id && m.UnitType == dragonDef.UnitType);
        if (unit == null)
            _context.MilitaryUnits.Add(new MilitaryUnit
            {
                KingdomId = kingdom.Id, UnitType = dragonDef.UnitType, Quantity = 1, InTraining = 0
            });
        else
            unit.Quantity += 1;

        return true;
    }

    private async Task<RaceDefinition> GetRaceAsync(string raceName)
    {
        return await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == raceName)
            ?? new RaceDefinition { Name = raceName };
    }

    /// <summary>
    /// Poziom najlepszego generała o danej cesze głównej, obecnego w domu i zdolnego
    /// do działania (docs/MECHANIKA.md §11 — „liczy się najlepszy"). 0 = brak.
    /// </summary>
    private async Task<int> BestHomeGeneralLevelAsync(int kingdomId, string primaryTrait)
    {
        var now = DateTime.UtcNow;
        var generals = await _context.Generals.AsNoTracking()
            .Where(g => g.KingdomId == kingdomId && g.PrimaryTrait == primaryTrait
                        && !g.IsPending && !g.IsImprisoned && !g.IsOutside
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .ToListAsync();
        return generals.Select(g => g.Level).DefaultIfEmpty(0).Max();
    }

    private static int TrainedMages(Kingdom kingdom)
    {
        var mages = kingdom.Professions?.FirstOrDefault(p => p.ProfessionType == "Magowie");
        return mages == null ? 0 : Math.Max(0, mages.WorkerCount - mages.NoviceCount);
    }

    private async Task<int> CountSpellsCastTodayAsync(int kingdomId)
    {
        var today = DateTime.UtcNow.Date;
        int queued = await _context.QueuedActions.CountAsync(a =>
            a.KingdomId == kingdomId && a.ActionType == "Spell" && a.CreatedAt >= today);
        int selfCast = await _context.ActiveSpells.CountAsync(s =>
            s.KingdomId == kingdomId && s.CastAt >= today);
        return queued + selfCast;
    }

    public async Task<List<SpellListItemDto>> GetAvailableSpellsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Professions)
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return new List<SpellListItemDto>();

        var race = await GetRaceAsync(kingdom.Race);
        var spells = await _context.SpellDefinitions.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        int castsToday = await CountSpellsCastTodayAsync(kingdom.Id);
        int mages = TrainedMages(kingdom);

        var result = new List<SpellListItemDto>();
        foreach (var spell in spells)
        {
            bool available = spell.RequiredRace != null
                ? spell.RequiredRace == kingdom.Race
                : race.MagicBooks >= spell.RequiredBooks;
            if (!available) continue;

            long cost = await CalculateSpellCostAsync(spell, kingdom, castsToday);
            string? reason = null;
            if (mages <= 0) reason = "Brak wyszkolonych magów.";
            else if (kingdom.TurnsAvailable <= 0) reason = "Brak tur.";
            else if (kingdom.Mana < cost) reason = "Za mało many.";

            result.Add(new SpellListItemDto
            {
                SpellType = spell.SpellType,
                DisplayName = spell.DisplayName,
                Category = spell.Category,
                Description = spell.Description,
                BaseCost = spell.ManaCost,
                CurrentCost = cost,
                IsLimited = spell.IsLimited,
                TargetType = spell.TargetType ?? "Self",
                CanCast = reason == null,
                CannotCastReason = reason
            });
        }

        return result;
    }

    public async Task<ServiceResult> CastSpellAsync(int userId, CastSpellDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Professions)
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var spell = await _context.SpellDefinitions
            .FirstOrDefaultAsync(s => s.SpellType == dto.SpellType);
        if (spell == null)
            return ServiceResult.Fail("Nieznane zaklęcie.");

        var race = await GetRaceAsync(kingdom.Race);
        bool available = spell.RequiredRace != null
            ? spell.RequiredRace == kingdom.Race
            : race.MagicBooks >= spell.RequiredBooks;
        if (!available)
            return ServiceResult.Fail("Twoja rasa nie zna tego zaklęcia (za mało ksiąg magii).");

        if (kingdom.IsFrozen)
            return ServiceResult.Fail("Twoje księstwo jest zamrożone — odmróź je, aby czarować.");

        int mages = TrainedMages(kingdom);
        if (mages <= 0)
            return ServiceResult.Fail("Nie masz wyszkolonych magów.");
        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Brak dostępnych tur.");

        int castsToday = await CountSpellsCastTodayAsync(kingdom.Id);
        long cost = await CalculateSpellCostAsync(spell, kingdom, castsToday);
        if (kingdom.Mana < cost)
            return ServiceResult.Fail($"Za mało many. Potrzeba: {cost}, posiadasz: {kingdom.Mana}.");

        // Przywołanie smoka: sprawdź limit zanim pobierzemy manę i turę
        if (spell.EffectType == "SummonDragon")
        {
            var (dragons, cap) = await GetDragonStatusAsync(kingdom);
            if (dragons >= cap)
                return ServiceResult.Fail($"Osiągnięto limit smoków ({cap}). Rozbuduj Smokodrap/Portal/Ministerstwo smoków lub rozwiń badania o smokach.");
        }

        // siła zaklęcia = liczba wyszkolonych magów (z bonusem rasowym)
        decimal powerVal = mages * (1m + race.BonusMages);

        // Budynki magiczne (Dracopedia §9, §14.2): Soczewka magiczna +20% siły zaklęć,
        // Kondensator magiczny +10%.
        bool HasB(string t) => kingdom.Buildings.Any(b => b.BuildingType == t && b.Quantity > 0 && !b.IsUnderConstruction);
        if (HasB("SoczewkaMagiczna")) powerVal *= 1.20m;
        if (HasB("KondensatorMagiczny")) powerVal *= 1.10m;

        // Metamagia Dżina: wzmocniona +10% siły, przyspieszona −25% siły
        if (kingdom.Race == "Dżin")
            powerVal *= kingdom.MetamagicMode switch
            {
                "Strengthened" => 1.10m,
                "Accelerated" => 0.75m,
                _ => 1m
            };

        // Nauka stosowana Człowieka: szkoła magiczna +10% siły zaklęć
        if (kingdom.Race == "Człowiek" && kingdom.AppliedScienceSchool == "Magic")
            powerVal *= 1.10m;

        // Generał Mag (docs/MECHANIKA.md §11): +lvl/(lvl+50) siły magicznej
        int mageGeneralLvl = await BestHomeGeneralLevelAsync(kingdom.Id, "Mag");
        if (mageGeneralLvl > 0)
            powerVal *= 1m + (decimal)mageGeneralLvl / (mageGeneralLvl + 50m);

        long power = (long)powerVal;

        kingdom.Mana -= cost;
        kingdom.TurnsAvailable--;

        bool selfTarget = spell.TargetType != "Enemy"
            || dto.TargetKingdomId == null || dto.TargetKingdomId == kingdom.Id;

        // Biała magia na sojusznika: pozytywne zaklęcie można rzucić na członka własnej
        // koalicji — zawiesza się na nim od razu, jak przy rzucaniu na siebie.
        bool isPositive = spell.Category is "Biała" or "Tarcze";
        if (selfTarget && isPositive && dto.TargetKingdomId.HasValue
            && dto.TargetKingdomId != kingdom.Id
            && spell.EffectType is not ("Mannamorphosis" or "SummonDragon" or "Sacrifice"
                or "SummonE2" or "SummonE1" or "SummonHoplites" or "SummonThieves"))
        {
            var ally = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);
            if (ally == null)
                return ServiceResult.Fail("Nie znaleziono celu.");
            if (kingdom.CoalitionId == null || ally.CoalitionId != kingdom.CoalitionId)
                return ServiceResult.Fail("Białą magię możesz rzucać tylko na siebie lub na członków własnej koalicji.");
            if (ally.IsFrozen)
                return ServiceResult.Fail("Cel jest zamrożony.");

            _context.ActiveSpells.Add(new ActiveSpell
            {
                KingdomId = ally.Id,
                SpellType = spell.SpellType,
                Power = (int)Math.Min(int.MaxValue, power),
                CastAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"Zaklęcie {spell.DisplayName} rzucone na sojusznika {ally.Name} (siła {power}). Koszt: {cost} many.");
        }

        if (selfTarget)
        {
            if (spell.TargetType == "Enemy")
                return ServiceResult.Fail("To zaklęcie wymaga wskazania wrogiego księstwa.");

            // Mannamorfoza: natychmiastowa zamiana many na złoto (200 złota / 1 manę)
            if (spell.EffectType == "Mannamorphosis")
            {
                long manaLeft = kingdom.Mana;
                long goldGained = manaLeft * 200;
                kingdom.Mana = 0;
                kingdom.Gold += goldGained;
                await _context.SaveChangesAsync();
                return ServiceResult.Ok($"Mannamorfoza zamieniła {manaLeft} many na {goldGained} złota.");
            }

            // Przywołanie smoka: dodaje 1 smoka do armii (limit sprawdzony wyżej)
            if (spell.EffectType == "SummonDragon")
            {
                bool added = await AddDragonAsync(kingdom);
                await _context.SaveChangesAsync();
                return added
                    ? ServiceResult.Ok($"Czerwony Smok dołączył do Twojej armii! Koszt: {cost} many.")
                    : ServiceResult.Fail("Twoja rasa nie potrafi przywoływać smoków.");
            }

            // Nekromancja: Ofiarowanie i przywołania armii z ciał
            if (spell.EffectType is "Sacrifice" or "SummonE2" or "SummonE1" or "SummonHoplites" or "SummonThieves")
            {
                var nres = await HandleNecromancyAsync(kingdom, spell, mages);
                await _context.SaveChangesAsync();
                return nres;
            }

            _context.ActiveSpells.Add(new ActiveSpell
            {
                KingdomId = kingdom.Id,
                SpellType = spell.SpellType,
                Power = (int)Math.Min(int.MaxValue, power),
                CastAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"Zaklęcie {spell.DisplayName} rzucone (siła {power}). Koszt: {cost} many.");
        }

        var target = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);
        if (target == null)
            return ServiceResult.Fail("Nie znaleziono celu.");
        if (target.IsProtected)
            return ServiceResult.Fail("Cel jest pod ochroną początkową.");
        if (target.IsFrozen)
            return ServiceResult.Fail("Cel jest zamrożony — nie można go zaatakować zaklęciem.");

        // Zaklęcia ofensywne WYŁĄCZNIE na cele w stanie wojny/zasadzki (docs/TODO.md A3):
        // obie strony muszą być w koalicjach, między którymi wypowiedziano wojnę.
        if (target.CoalitionId == kingdom.CoalitionId && kingdom.CoalitionId != null)
            return ServiceResult.Fail("Nie możesz rzucać zaklęć ofensywnych na członka własnej koalicji.");
        if (kingdom.CoalitionId == null || target.CoalitionId == null
            || !await WarHelper.AreAtWarAsync(_context, kingdom.CoalitionId, target.CoalitionId))
            return ServiceResult.Fail("Zaklęcia ofensywne możesz rzucać tylko na księstwa, którym Twoja koalicja wypowiedziała wojnę lub zasadzkę.");

        _context.QueuedActions.Add(new QueuedAction
        {
            KingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            ActionType = "Spell",
            ActionData = JsonSerializer.Serialize(new SpellAttackData { SpellType = spell.SpellType, Power = power }),
            ScheduledFor = GetNextResetTime(),
            Status = "Pending"
        });

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zaklęcie {spell.DisplayName} zostanie rzucone na {target.Name} podczas przeliczenia (siła {power}).");
    }

    public async Task ExecuteSpellAsync(QueuedAction action)
    {
        var data = JsonSerializer.Deserialize<SpellAttackData>(action.ActionData!);
        if (data == null) return;

        var spell = await _context.SpellDefinitions.FirstOrDefaultAsync(s => s.SpellType == data.SpellType);
        var attacker = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == action.KingdomId);
        var defender = await _context.Kingdoms
            .Include(k => k.Professions)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.ActiveSpells)
            .FirstOrDefaultAsync(k => k.Id == action.TargetKingdomId);
        if (spell == null || attacker == null || defender == null) return;

        var defenderRace = await GetRaceAsync(defender.Race);

        // Limit zaklęć niszczących na cel za przeliczenie (rasa celu: std 2, Krasnolud 1, Ent 3, Olbrzym 4)
        if (spell.IsLimited)
        {
            var today = DateTime.UtcNow.Date;
            var executedToday = await _context.QueuedActions
                .Where(a => a.TargetKingdomId == defender.Id && a.ActionType == "Spell"
                            && a.Status == "Executed" && a.ExecutedAt >= today)
                .ToListAsync();
            int limitedLanded = 0;
            foreach (var ex in executedToday)
            {
                var exData = JsonSerializer.Deserialize<SpellAttackData>(ex.ActionData ?? "{}");
                if (exData != null && await _context.SpellDefinitions
                        .AnyAsync(s => s.SpellType == exData.SpellType && s.IsLimited))
                    limitedLanded++;
            }
            if (limitedLanded >= defenderRace.LimitedSpellsPerRecalc)
            {
                ReportMagic(attacker, defender, spell, "Blocked",
                    "Cel osiągnął limit zaklęć niszczących w tym przeliczeniu.");
                await _context.SaveChangesAsync();
                return;
            }
        }

        // Pojedynek magiczny: siła rzucającego vs obrona magiczna celu
        long attackPower = data.Power;
        long defensePower = TrainedMages(defender);
        var antimagic = defender.ActiveSpells.FirstOrDefault(s => s.SpellType == "TarczaAntymagiczna");
        if (antimagic != null) defensePower += antimagic.Power;

        // Pakty magiczne: magowie partnera pomagają bronić (Dżin z Pałacem +5% — uproszczone do bazy)
        var magicPartners = await PactService.GetActivePactPartnersAsync(_context, defender.Id, "Magiczny");
        if (magicPartners.Count > 0)
        {
            decimal efficiency = PactService.PactEfficiency(magicPartners.Count);
            foreach (var partner in magicPartners)
                defensePower += (long)(TrainedMages(partner) * efficiency);
        }
        var mirror = defender.ActiveSpells.FirstOrDefault(s => s.SpellType == "ZwierciadloMagiczne");

        double chance = Helpers.BattleCalculator.ThiefSuccessChance(attackPower, Math.Max(1, defensePower));
        bool success = Random.Shared.NextDouble() < chance;

        if (!success)
        {
            ReportMagic(attacker, defender, spell, "Repelled",
                mirror != null ? "Zaklęcie odbite Zwierciadłem magicznym." : "Magiczna obrona celu odparła zaklęcie.");
            await _context.SaveChangesAsync();
            return;
        }

        string effectInfo = ApplySpellEffect(spell, defender, defenderRace, attackPower);
        ReportMagic(attacker, defender, spell, "Success", effectInfo);
        await _context.SaveChangesAsync();
    }

    /// <summary>Efekty zaklęć ofensywnych (procenty przybliżone, modyfikatory rasowe z manuala).</summary>
    private string ApplySpellEffect(SpellDefinition spell, Kingdom defender, RaceDefinition defenderRace, long power)
    {
        switch (spell.EffectType)
        {
            case "EagleEye": // Sokole Oko — wywiad magiczny
                {
                    int e = defender.MilitaryUnits.Where(u => !u.UnitType.EndsWith("_Smok")).Sum(u => u.Quantity);
                    int mages = TrainedMages(defender);
                    return $"Sokole Oko: {defender.Name} — armia {e}, magowie {mages}, " +
                           $"ziemia {defender.Land}, tury {defender.TurnsAvailable}.";
                }
            case "PopulationDamage": // Zaraza (PL: Olbrzym odporny; Nekromant — nieumarli)
                if (defender.Race is "Olbrzym" or "Nekromant") return "Cel odporny na Zarazę.";
                {
                    int killed = (int)(defender.Population * 0.03);
                    defender.Population = Math.Max(100, defender.Population - killed);
                    return $"Zaraza zabiła {killed} mieszkańców.";
                }
            case "FoodDamage": // Szarańcza (PL: niszczy 9% zapasów jedzenia)
                {
                    long eaten = (long)(defender.Food * 0.09);
                    defender.Food -= eaten;
                    return $"Szarańcza pożarła {eaten} jedzenia (głód potrwa do zdjęcia zaklęcia).";
                }
            case "WorkerDamage": // Huragan (PL: zabija 4% ludzi w profesjach)
                {
                    int killedTotal = 0;
                    foreach (var prof in defender.Professions.Where(p => p.WorkerCount > 0))
                    {
                        int killed = (int)(prof.WorkerCount * 0.04);
                        prof.WorkerCount -= killed;
                        killedTotal += killed;
                    }
                    defender.Population = Math.Max(100, defender.Population - killedTotal);
                    return $"Huragan zabił {killedTotal} pracowników.";
                }
            case "DoomLegions": // Klątwa Padłych Legionów
                {
                    var legions = defender.ActiveSpells.FirstOrDefault(s => s.SpellType == "PadleLegiony");
                    if (legions == null) return "Cel nie ma Padłych legionów.";
                    legions.Power -= (int)Math.Min(int.MaxValue, power);
                    if (legions.Power <= 0) _context.ActiveSpells.Remove(legions);
                    return $"Klątwa odesłała duchy do grobów (osłabiono legiony o {power}).";
                }
            case "SupplyDamage": // Zniszczenie zapasów (PL: 20% zasobów)
                {
                    decimal pct = defender.Race == "Hobbit" ? 0.10m : 0.20m;
                    long gold = (long)(defender.Gold * pct);
                    long food = (long)(defender.Food * pct);
                    long stone = (long)(defender.Stone * pct);
                    long weapons = (long)(defender.Weapons * pct);
                    defender.Gold -= gold; defender.Food -= food;
                    defender.Stone -= stone; defender.Weapons -= weapons;
                    return $"Zniszczono zapasy: {gold} złota, {food} jedzenia, {stone} kamienia, {weapons} broni.";
                }
            case "ArmyDamage": // Ognisty Deszcz
                {
                    decimal pct = 0.10m;
                    if (defender.Race == "Elf") pct *= 0.5m;   // Elf: połowa strat
                    if (defender.Race == "Ent") pct *= 2m;     // Ent: podwójne straty
                    int totalKilled = 0;
                    foreach (var unit in defender.MilitaryUnits.Where(u =>
                                 u.Quantity > 0 && !u.UnitType.EndsWith("_Smok") && !u.UnitType.EndsWith("_Zlodziej")))
                    {
                        int killed = (int)(unit.Quantity * pct);
                        unit.Quantity -= killed;
                        totalKilled += killed;
                    }
                    return $"Ognisty deszcz zabił {totalKilled} żołnierzy.";
                }
            case "ThiefDamage": // Spopielenie złodziei (PL: 5–10%; Goblin całkowicie odporny)
                {
                    if (defender.Race == "Goblin") return "Gobliny są odporne na Spopielenie złodziei.";
                    var thieves = defender.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
                    if (thieves == null || thieves.Quantity == 0) return "Cel nie ma złodziei.";
                    int killed = (int)(thieves.Quantity * (0.05 + Random.Shared.NextDouble() * 0.05));
                    thieves.Quantity -= killed;
                    return $"Spopielono {killed} złodziei.";
                }
            case "BuildingDamage": // Trzęsienie Ziemi (PL: 1–2% infrastruktury, 50%·x na budynek specjalny)
            case "DragonBreath":   // Smoczy Oddech (PL: 1–2% budynków, 3–5% armii, 5–10% ludności, 50% na specjalny)
                {
                    decimal dmgMod = defender.Race == "Krasnolud" ? 0.75m : 1m; // Krasnolud: 75% szkód
                    decimal pctBuildings = (0.01m + (decimal)Random.Shared.NextDouble() * 0.01m) * dmgMod;
                    int destroyed = 0;
                    foreach (var b in defender.Buildings.Where(b => b.Quantity > 0 && b.Definition is { IsSpecial: false }))
                    {
                        int lost = (int)Math.Ceiling(b.Quantity * pctBuildings);
                        b.Quantity -= lost;
                        destroyed += lost;
                    }

                    // szansa na zwalenie budynku specjalnego
                    double sbChance = spell.EffectType == "DragonBreath" ? 0.5 : 0.5 *
                        (defender.Buildings.Count(b => b.Definition is { IsSpecial: true } && b.Quantity > 0) / 38.0);
                    string extra = "";
                    if (Random.Shared.NextDouble() < sbChance)
                    {
                        var sb = defender.Buildings
                            .Where(b => b.Definition is { IsSpecial: true } && b.Quantity > 0)
                            .OrderBy(_ => Random.Shared.Next())
                            .FirstOrDefault();
                        if (sb != null)
                        {
                            sb.Quantity = 0;
                            extra = $", runął budynek specjalny: {sb.Definition!.DisplayName}";
                        }
                    }

                    if (spell.EffectType == "DragonBreath")
                    {
                        int popKilled = (int)(defender.Population * (0.05m + (decimal)Random.Shared.NextDouble() * 0.05m) * dmgMod);
                        defender.Population = Math.Max(100, defender.Population - popKilled);
                        decimal armyPct = (0.03m + (decimal)Random.Shared.NextDouble() * 0.02m) * dmgMod;
                        int armyKilled = 0;
                        foreach (var unit in defender.MilitaryUnits.Where(u =>
                                     u.Quantity > 0 && !u.UnitType.EndsWith("_Smok") && !u.UnitType.EndsWith("_Zlodziej")))
                        {
                            int lost = (int)(unit.Quantity * armyPct);
                            unit.Quantity -= lost;
                            armyKilled += lost;
                        }
                        int popLoss = defender.Race == "Hobbit" ? 5 : 10;
                        defender.Popularity = Math.Max(0, defender.Popularity - popLoss);
                        extra += $", zabił {popKilled} mieszkańców i {armyKilled} żołnierzy, popularność −{popLoss}";
                    }
                    return $"{spell.DisplayName} zburzył {destroyed} budynków{extra}.";
                }
            case "MachineDamage": // Chochliki — niszczą machiny wojenne (docs/MECHANIKA.md §14.2)
                {
                    if (defender.Race == "Gnom")
                        return "Gnomy są odporne na Chochliki.";
                    if (defender.Race == "Goblin")
                        return "Machiny Goblinów są niezniszczalne — Chochliki nic nie wskórały.";
                    var machines = defender.MilitaryUnits
                        .Where(u => u.UnitType.EndsWith("_Machina") && u.Quantity > 0)
                        .ToList();
                    if (machines.Count == 0)
                        return "Cel nie ma machin wojennych.";
                    // 10–20% machin zniszczonych
                    decimal machinePct = 0.10m + (decimal)Random.Shared.NextDouble() * 0.10m;
                    int machinesDestroyed = 0;
                    foreach (var m in machines)
                    {
                        int lost = Math.Max(1, (int)(m.Quantity * machinePct));
                        m.Quantity = Math.Max(0, m.Quantity - lost);
                        machinesDestroyed += lost;
                    }
                    return $"Chochliki zniszczyły {machinesDestroyed} machin wojennych.";
                }
            default:
                // negatywne uroki (Pech, Zły humor, Somnambulizm…) — zawieszane na celu
                if (spell.EffectType == "PopularityDebuff" && defender.Race == "Hobbit")
                    return "Hobbici są odporni na Zły humor.";
                if (spell.EffectType == "DefenseDebuff" && defender.Race == "Ent")
                    return "Enty są odporne na Słabość.";
                if (spell.EffectType == "GrowthDebuff" && spell.SpellType == "Kastracja" && defender.Race == "Nekromant")
                    return "Nekromanci są odporni na Kastrację.";
                _context.ActiveSpells.Add(new ActiveSpell
                {
                    KingdomId = defender.Id,
                    SpellType = spell.SpellType,
                    Power = (int)Math.Min(int.MaxValue, power),
                    CastAt = DateTime.UtcNow
                });
                return $"{spell.DisplayName} ciąży teraz nad celem (siła {power}).";
        }
    }

    private void ReportMagic(Kingdom attacker, Kingdom defender, SpellDefinition spell, string result, string info)
    {
        _context.BattleReports.Add(new BattleReport
        {
            AttackerKingdomId = attacker.Id,
            DefenderKingdomId = defender.Id,
            BattleType = "Magic",
            Result = result,
            AttackerLosses = JsonSerializer.Serialize(new { spell = spell.DisplayName }),
            DefenderLosses = JsonSerializer.Serialize(new { info }),
            OccurredAt = DateTime.UtcNow
        });
    }

    // ====================== ZŁODZIEJE ======================

    public async Task<List<ThiefActionListItemDto>> GetThiefActionsAsync()
    {
        return await _context.ThiefActionDefinitions.AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new ThiefActionListItemDto
            {
                ActionType = t.ActionType,
                DisplayName = t.DisplayName,
                Description = t.Description,
                ThievesRequired = t.ThievesRequired
            })
            .ToListAsync();
    }

    public async Task<ServiceResult> SendThievesAsync(int userId, SendThievesDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.MilitaryUnits)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var actionDef = await _context.ThiefActionDefinitions
            .FirstOrDefaultAsync(t => t.ActionType == dto.ActionType);
        if (actionDef == null)
            return ServiceResult.Fail("Nieznana akcja złodziejska.");

        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Brak dostępnych tur.");
        if (dto.Thieves < actionDef.ThievesRequired)
            return ServiceResult.Fail($"Akcja wymaga co najmniej {actionDef.ThievesRequired} złodziei.");

        var thievesUnit = kingdom.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
        if (thievesUnit == null || thievesUnit.Quantity < dto.Thieves)
            return ServiceResult.Fail($"Masz tylko {thievesUnit?.Quantity ?? 0} złodziei.");

        var target = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);
        if (target == null)
            return ServiceResult.Fail("Nie znaleziono celu.");
        if (target.Id == kingdom.Id)
            return ServiceResult.Fail("Nie możesz okradać samego siebie.");
        if (target.IsProtected)
            return ServiceResult.Fail("Cel jest pod ochroną początkową.");
        if (target.IsFrozen)
            return ServiceResult.Fail("Cel jest zamrożony — nie można wysłać na niego złodziei.");

        // Akcje złodziejskie WYŁĄCZNIE na cele w stanie wojny/zasadzki (docs/TODO.md A3)
        if (target.CoalitionId == kingdom.CoalitionId && kingdom.CoalitionId != null)
            return ServiceResult.Fail("Nie możesz wysyłać złodziei na członka własnej koalicji.");
        if (kingdom.CoalitionId == null || target.CoalitionId == null
            || !await WarHelper.AreAtWarAsync(_context, kingdom.CoalitionId, target.CoalitionId))
            return ServiceResult.Fail("Złodziei możesz wysyłać tylko na księstwa, którym Twoja koalicja wypowiedziała wojnę lub zasadzkę.");

        // złodzieje wychodzą z księstwa — wracają (ci, co przeżyją) po przeliczeniu
        thievesUnit.Quantity -= dto.Thieves;
        kingdom.TurnsAvailable--;

        _context.QueuedActions.Add(new QueuedAction
        {
            KingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            ActionType = "ThiefAction",
            ActionData = JsonSerializer.Serialize(new ThiefAttackData { ActionType = dto.ActionType, Thieves = dto.Thieves }),
            ScheduledFor = GetNextResetTime(),
            Status = "Pending"
        });

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"{dto.Thieves} złodziei wyruszyło na akcję „{actionDef.DisplayName}” przeciw {target.Name}.");
    }

    public async Task ExecuteThiefActionAsync(QueuedAction action)
    {
        var data = JsonSerializer.Deserialize<ThiefAttackData>(action.ActionData!);
        if (data == null) return;

        var attacker = await _context.Kingdoms
            .Include(k => k.MilitaryUnits)
            .FirstOrDefaultAsync(k => k.Id == action.KingdomId);
        var defender = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .FirstOrDefaultAsync(k => k.Id == action.TargetKingdomId);
        var actionDef = await _context.ThiefActionDefinitions
            .FirstOrDefaultAsync(t => t.ActionType == data.ActionType);
        if (attacker == null || defender == null || actionDef == null) return;

        var attackerRace = await GetRaceAsync(attacker.Race);
        var defenderRace = await GetRaceAsync(defender.Race);

        // siły złodziejskie z modyfikatorami rasowymi (+ szkoła złodziejska Człowieka)
        decimal thiefBonus = 0m;
        if (attacker.Race == "Człowiek" && attacker.AppliedScienceSchool == "Thief") thiefBonus += 0.10m;
        long attackPower = (long)(data.Thieves * (1m + attackerRace.ThiefPowerModifier + thiefBonus)
                                  * actionDef.SuccessBaseRate);
        var defThieves = defender.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
        long defensePower = (long)((defThieves?.Quantity ?? 0) * (1m + defenderRace.ThiefPowerModifier));

        // Generał Złodziej (docs/MECHANIKA.md §11): +lvl/(lvl+50) siły złodziei — po obu stronach
        int atkThiefGenLvl = await BestHomeGeneralLevelAsync(attacker.Id, "Zlodziej");
        if (atkThiefGenLvl > 0)
            attackPower = (long)(attackPower * (1m + (decimal)atkThiefGenLvl / (atkThiefGenLvl + 50m)));
        int defThiefGenLvl = await BestHomeGeneralLevelAsync(defender.Id, "Zlodziej");
        if (defThiefGenLvl > 0)
            defensePower = (long)(defensePower * (1m + (decimal)defThiefGenLvl / (defThiefGenLvl + 50m)));

        // Pakty złodziejskie: złodzieje partnera (w domu) pomagają bronić
        var thiefPartners = await PactService.GetActivePactPartnersAsync(_context, defender.Id, "Zlodziejski");
        if (thiefPartners.Count > 0)
        {
            decimal efficiency = PactService.PactEfficiency(thiefPartners.Count);
            foreach (var partner in thiefPartners)
            {
                int partnerThieves = partner.MilitaryUnits
                    .Where(u => u.UnitType.EndsWith("_Zlodziej"))
                    .Sum(u => u.Quantity);
                defensePower += (long)(partnerThieves * efficiency);
            }
        }

        double chance = Helpers.BattleCalculator.ThiefSuccessChance(attackPower, Math.Max(1, defensePower));
        bool success = Random.Shared.NextDouble() < chance;

        // straty: ~10% przy równowadze (5% przy sukcesie, do 20% przy porażce)
        double lossRate = success ? 0.05 : 0.20;
        int losses = (int)(data.Thieves * lossRate);
        int survivors = data.Thieves - losses;

        var attThieves = attacker.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
        if (attThieves != null) attThieves.Quantity += survivors;

        string effectInfo;
        if (!success)
        {
            // nieudana akcja — obrońca traci niewielu złodziei
            if (defThieves != null)
                defThieves.Quantity = Math.Max(0, defThieves.Quantity - (int)(defThieves.Quantity * 0.02));
            effectInfo = "Akcja wykryta i odparta przez złodziei obrońcy.";
        }
        else
        {
            effectInfo = ApplyThiefEffect(actionDef, attacker, defender, data.Thieves);
        }

        _context.BattleReports.Add(new BattleReport
        {
            AttackerKingdomId = attacker.Id,
            DefenderKingdomId = defender.Id,
            BattleType = "Thief",
            Result = success ? "Success" : "Failed",
            AttackerLosses = JsonSerializer.Serialize(new { thievesLost = losses, action = actionDef.DisplayName }),
            DefenderLosses = JsonSerializer.Serialize(new { info = effectInfo }),
            OccurredAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    private string ApplyThiefEffect(ThiefActionDefinition actionDef, Kingdom attacker, Kingdom defender, int thieves)
    {
        switch (actionDef.EffectType)
        {
            case "Spy":
                return $"Wywiad: {defender.Name} — ziemia {defender.Land}, ludność {defender.Population}, " +
                       $"złoto {defender.Gold}, jedzenie {defender.Food}, armia {defender.MilitaryUnits.Sum(u => u.Quantity)}, " +
                       $"tury {defender.TurnsAvailable}, popularność {defender.Popularity}.";
            case "StealSupplies":
                {
                    decimal pct = Math.Min(0.10m, thieves / 10000m + 0.02m);
                    long gold = (long)(defender.Gold * pct);
                    long food = (long)(defender.Food * pct);
                    defender.Gold -= gold; attacker.Gold += gold;
                    defender.Food -= food; attacker.Food += food;
                    return $"Skradziono {gold} złota i {food} jedzenia.";
                }
            case "Revolt":
                {
                    int drop = defender.Race == "Hobbit" ? 8 : 15; // Hobbit: efekt połowiczny
                    defender.Popularity = Math.Max(0, defender.Popularity - drop);
                    return $"Rewolta obniżyła popularność o {drop}.";
                }
            case "DemolishBuildings":
                {
                    int toDestroy = Math.Max(1, thieves / 100);
                    int destroyed = 0;
                    foreach (var b in defender.Buildings.Where(b => b.Quantity > 0 && b.Definition is { IsSpecial: false }))
                    {
                        int lost = Math.Min(b.Quantity, toDestroy - destroyed);
                        b.Quantity -= lost;
                        destroyed += lost;
                        if (destroyed >= toDestroy) break;
                    }
                    return $"Zburzono {destroyed} budynków.";
                }
            case "ThiefWar":
                {
                    var defThieves = defender.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
                    if (defThieves == null || defThieves.Quantity == 0) return "Cel nie ma złodziei.";
                    int killed = (int)(defThieves.Quantity * 0.15);
                    defThieves.Quantity -= killed;
                    return $"W wojnie gangów zginęło {killed} złodziei obrońcy.";
                }
            case "KillMages":
                {
                    var mages = defender.Professions.FirstOrDefault(p => p.ProfessionType == "Magowie");
                    if (mages == null || mages.WorkerCount == 0) return "Cel nie ma magów.";
                    int killed = Math.Max(1, (int)(mages.WorkerCount * 0.05));
                    mages.WorkerCount -= killed;
                    return $"Zamordowano {killed} magów.";
                }
            case "KillPeople":
                {
                    int killed = (int)(defender.Population * 0.03);
                    defender.Population = Math.Max(100, defender.Population - killed);
                    return $"Zabito {killed} mieszkańców.";
                }
            case "DrunkArmy":
                {
                    int affected = defender.MilitaryUnits
                        .Where(u => !u.UnitType.EndsWith("_Smok") && !u.UnitType.EndsWith("_Zlodziej"))
                        .Sum(u => u.Quantity) / 4;
                    if (affected <= 0) return "Cel nie ma armii do upicia.";
                    // 25% armii upite → obrona −25% do końca najbliższego przeliczenia
                    // (flaga zerowana w DailyResetService po wykonaniu wszystkich ataków).
                    defender.DrunkArmyPct = Math.Max(defender.DrunkArmyPct, 25);
                    return $"Upito ~{affected} żołnierzy — armia broni się o 25% słabiej do najbliższego przeliczenia.";
                }
            case "KillGeneral":
                {
                    var targetGeneral = _context.Generals.Local
                        .Where(g => g.KingdomId == defender.Id && !g.IsImprisoned && !g.IsOutside)
                        .OrderBy(_ => Random.Shared.Next())
                        .FirstOrDefault()
                        ?? _context.Generals
                            .Where(g => g.KingdomId == defender.Id && !g.IsImprisoned && !g.IsOutside)
                            .AsEnumerable()
                            .OrderBy(_ => Random.Shared.Next())
                            .FirstOrDefault();
                    if (targetGeneral == null) return "Cel nie ma generałów w domu.";
                    _context.Generals.Remove(targetGeneral);
                    return $"Zamordowano generała {targetGeneral.Name} (poziom {targetGeneral.Level}).";
                }
            case "KidnapGeneral":
                {
                    var targetGeneral = _context.Generals
                        .Where(g => g.KingdomId == defender.Id && !g.IsImprisoned && !g.IsOutside)
                        .AsEnumerable()
                        .OrderBy(_ => Random.Shared.Next())
                        .FirstOrDefault();
                    if (targetGeneral == null) return "Cel nie ma generałów w domu.";
                    targetGeneral.IsImprisoned = true;
                    return $"Porwano generała {targetGeneral.Name} (poziom {targetGeneral.Level}) — trafił do lochów.";
                }
            default:
                return "Akcja wykonana.";
        }
    }

    private static DateTime GetNextResetTime()
    {
        var now = DateTime.UtcNow;
        var today5am = DateTime.UtcNow.Date.AddHours(4); // 4:00 UTC = 5:00 CET
        return now < today5am ? today5am : today5am.AddDays(1);
    }
}
