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

    public async Task<ServiceResult> QueueAttackAsync(int userId, AttackDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.MilitaryUnits)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        if (kingdom.IsFrozen)
            return ServiceResult.Fail("Twoje księstwo jest zamrożone — odmróź je, aby atakować.");

        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Brak dostępnych tur.");

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
        foreach (var unit in dto.Units)
        {
            var militaryUnit = kingdom.MilitaryUnits.FirstOrDefault(m => m.UnitType == unit.Key);
            if (militaryUnit == null || militaryUnit.Quantity < unit.Value)
                return ServiceResult.Fail($"Za mało jednostek typu {unit.Key}.");
        }

        // Zakolejkuj atak
        var action = new QueuedAction
        {
            KingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            ActionType = "MilitaryAttack",
            ActionData = JsonSerializer.Serialize(new MilitaryAttackData { Units = dto.Units }),
            ScheduledFor = GetNextResetTime(),
            Status = "Pending"
        };

        _context.QueuedActions.Add(action);

        // Zużyj turę
        kingdom.TurnsAvailable--;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"Atak na {target.Name} został zakolejkowany. Wykonanie podczas najbliższego przeliczenia.");
    }

    public async Task<List<BattleReportDto>> GetBattleReportsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return new List<BattleReportDto>();

        var reports = await _context.BattleReports
            .Where(b => b.AttackerKingdomId == kingdom.Id || b.DefenderKingdomId == kingdom.Id)
            .Include(b => b.AttackerKingdom)
            .Include(b => b.DefenderKingdom)
            .OrderByDescending(b => b.OccurredAt)
            .Take(50)
            .Select(b => new BattleReportDto
            {
                Id = b.Id,
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

        return reports;
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

        // Krwawa magia Wampira: eliksir Ataku (+7%/lvl)
        if (attacker.Race == "Wampir" && attacker.BloodElixirAttack > 0)
            attackPower = (long)(attackPower * (1m + 0.07m * attacker.BloodElixirAttack));

        // Generałowie: Wódz zwiększa atak o lvl%, Obrońca (najlepszy w domu) obronę o lvl%
        var now = DateTime.UtcNow;
        var attackerGenerals = await _context.Generals
            .Where(g => g.KingdomId == attacker.Id && !g.IsImprisoned
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .ToListAsync();
        var defenderGenerals = await _context.Generals
            .Where(g => g.KingdomId == defender.Id && !g.IsImprisoned && !g.IsOutside
                        && (g.WoundedUntil == null || g.WoundedUntil <= now))
            .ToListAsync();

        var leadingGeneral = attackerGenerals
            .Where(g => g.PrimaryTrait == "Wodz" && !g.IsOutside)
            .OrderByDescending(g => g.Experience)
            .FirstOrDefault();
        if (leadingGeneral != null)
        {
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

        bool attackerWins = attackPower > defensePower;

        // Straty (~15% przy równowadze; modyfikatory rasowe: Krasnolud −25%, Ent −50%)
        var attackerCasualties = BattleCalculator.CalculateCasualties(
            attackData.Units, attackPower, defensePower, attackerWins, attackerRace);
        var defenderCasualties = BattleCalculator.CalculateDefenderCasualties(
            defender.MilitaryUnits, attackPower, defensePower, attackerWins, defenderRace);

        // Krwawa magia Wampira: eliksir Krwiożerczości zwiększa straty wroga (+12,5%/lvl)
        if (attacker.Race == "Wampir" && attacker.BloodElixirBloodlust > 0)
        {
            decimal mult = 1m + 0.125m * attacker.BloodElixirBloodlust;
            defenderCasualties = defenderCasualties.ToDictionary(c => c.Key, c => (int)(c.Value * mult));
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

        // Gniew Enta: Ent, który poniósł straty, wpada w szał (+100% ataku do przeliczenia)
        if (defender.Race == "Ent" && defenderDead > 0) defender.EntWrathActive = true;

        // Krwawa magia Wampira: punkty krwi za zabitych wrogów (max 50×obszar)
        if (attacker.Race == "Wampir" && defenderDead > 0)
        {
            long cap = 50L * attacker.Land;
            attacker.BloodPoints = Math.Min(cap, attacker.BloodPoints + defenderDead * 10);
        }

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
            .Include(r => r.Tech)
            .Select(r => r.Tech!.EffectValue)
            .DefaultIfEmpty(0m)
            .MaxAsync();

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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
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

        // Metamagia Dżina: wzmocniona +10% siły, przyspieszona −25% siły
        if (kingdom.Race == "Dżin")
            powerVal *= kingdom.MetamagicMode switch
            {
                "Strengthened" => 1.10m,
                "Accelerated" => 0.75m,
                _ => 1m
            };

        // Krwawa magia Wampira: eliksir Skupienia (+3%/lvl siły magów)
        if (kingdom.Race == "Wampir" && kingdom.BloodElixirFocus > 0)
            powerVal *= 1m + 0.03m * kingdom.BloodElixirFocus;

        long power = (long)powerVal;

        kingdom.Mana -= cost;
        kingdom.TurnsAvailable--;

        bool selfTarget = spell.TargetType != "Enemy"
            || dto.TargetKingdomId == null || dto.TargetKingdomId == kingdom.Id;

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

        // Zaklęcia ofensywne na członka obcej koalicji tylko w stanie wojny (docs/MECHANIKA.md §14.4)
        if (target.CoalitionId != null && target.CoalitionId != kingdom.CoalitionId
            && !await WarHelper.AreAtWarAsync(_context, kingdom.CoalitionId, target.CoalitionId))
            return ServiceResult.Fail("Możesz rzucać zaklęcia na członka obcej koalicji tylko w stanie wojny.");

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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
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

        // siły złodziejskie z modyfikatorami rasowymi (+ eliksir Złodziei Wampira +5%/lvl)
        decimal thiefBonus = attacker.Race == "Wampir" ? 0.05m * attacker.BloodElixirThief : 0m;
        long attackPower = (long)(data.Thieves * (1m + attackerRace.ThiefPowerModifier + thiefBonus)
                                  * actionDef.SuccessBaseRate);
        var defThieves = defender.MilitaryUnits.FirstOrDefault(u => u.UnitType.EndsWith("_Zlodziej"));
        long defensePower = (long)((defThieves?.Quantity ?? 0) * (1m + defenderRace.ThiefPowerModifier));

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
                    // upita armia: Wampir zabija 25% upitych; pozostali — osłabienie odnotowane w raporcie
                    int affected = defender.MilitaryUnits
                        .Where(u => !u.UnitType.EndsWith("_Smok") && !u.UnitType.EndsWith("_Zlodziej"))
                        .Sum(u => u.Quantity) / 4;
                    if (attacker.Race == "Wampir")
                    {
                        int killedTotal = 0;
                        foreach (var unit in defender.MilitaryUnits.Where(u =>
                                     u.Quantity > 0 && !u.UnitType.EndsWith("_Smok") && !u.UnitType.EndsWith("_Zlodziej")))
                        {
                            int killed = (int)(unit.Quantity * 0.0625); // 25% z 1/4 upitych
                            unit.Quantity -= killed;
                            killedTotal += killed;
                        }
                        return $"Upito ~{affected} żołnierzy; {killedTotal} zapiło się na śmierć (Wampir).";
                    }
                    return $"Upito ~{affected} żołnierzy — nie staną do obrony przy najbliższym przeliczeniu.";
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
