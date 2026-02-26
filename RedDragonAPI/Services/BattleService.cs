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

        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Brak dostępnych tur.");

        var target = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);

        if (target == null)
            return ServiceResult.Fail("Nie znaleziono celu ataku.");

        if (target.Id == kingdom.Id)
            return ServiceResult.Fail("Nie możesz atakować samego siebie.");

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
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .FirstOrDefaultAsync(k => k.Id == action.KingdomId);

        var defender = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .FirstOrDefaultAsync(k => k.Id == action.TargetKingdomId);

        if (attacker == null || defender == null)
            return new BattleResult { Success = false };

        // Oblicz siły
        int attackPower = BattleCalculator.CalculateAttackPower(
            attacker.MilitaryUnits, attackData.Units, attacker.Researches);
        int defensePower = BattleCalculator.CalculateDefensePower(
            defender, defender.Researches);

        // Losowość
        double randomFactor = BattleCalculator.GetRandomFactor();
        attackPower = (int)(attackPower * randomFactor);

        bool attackerWins = attackPower > defensePower;

        // Straty
        var attackerCasualties = BattleCalculator.CalculateCasualties(
            attackData.Units, attackPower, defensePower, attackerWins);
        var defenderCasualties = BattleCalculator.CalculateDefenderCasualties(
            defender.MilitaryUnits, attackPower, defensePower, attackerWins);

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

        int landCaptured = 0;
        ResourcesStolen? resourcesStolen = null;

        if (attackerWins)
        {
            landCaptured = BattleCalculator.CalculateLandCaptured(attackPower, defensePower, defender.Land);
            resourcesStolen = BattleCalculator.CalculateResourcesStolen(defender);

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
        }

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

    public async Task ExecuteThiefActionAsync(QueuedAction action)
    {
        // Implementacja akcji złodziejskich - uproszczona wersja
        action.Status = "Executed";
        action.ExecutedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ExecuteSpellAsync(QueuedAction action)
    {
        // Implementacja czarów - uproszczona wersja
        action.Status = "Executed";
        action.ExecutedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static DateTime GetNextResetTime()
    {
        var now = DateTime.UtcNow;
        var today5am = DateTime.UtcNow.Date.AddHours(4); // 4:00 UTC = 5:00 CET
        return now < today5am ? today5am : today5am.AddDays(1);
    }
}
