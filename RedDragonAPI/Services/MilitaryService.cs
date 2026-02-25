using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class MilitaryService : IMilitaryService
{
    private readonly ApplicationDbContext _context;

    public MilitaryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UnitDefinitionDto>> GetAvailableUnitsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Include(k => k.Researches)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return new List<UnitDefinitionDto>();

        var definitions = await _context.UnitDefinitions
            .Where(u => u.Race == kingdom.Race)
            .ToListAsync();

        return definitions.Select(d =>
        {
            var (canRecruit, reason) = CheckCanRecruit(kingdom, d);
            return new UnitDefinitionDto
            {
                Id = d.Id,
                UnitType = d.UnitType,
                DisplayName = d.DisplayName,
                Description = d.Description,
                CostGold = d.CostGold,
                CostIron = d.CostIron,
                CostWood = d.CostWood,
                CostFood = d.CostFood,
                AttackPower = d.AttackPower,
                DefensePower = d.DefensePower,
                Upkeep = d.Upkeep,
                RequiredBuilding = d.RequiredBuilding,
                RequiredTech = d.RequiredTech,
                TrainingTime = d.TrainingTime,
                CanRecruit = canRecruit,
                CannotRecruitReason = reason
            };
        }).ToList();
    }

    public async Task<List<MilitaryUnitDto>> GetMyArmyAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return new List<MilitaryUnitDto>();

        return kingdom.MilitaryUnits.Select(m => new MilitaryUnitDto
        {
            Id = m.Id,
            UnitType = m.UnitType,
            DisplayName = m.Definition?.DisplayName ?? m.UnitType,
            Description = m.Definition?.Description,
            Quantity = m.Quantity,
            InTraining = m.InTraining,
            TrainingCompletesAt = m.TrainingCompletesAt,
            AttackPower = m.Definition?.AttackPower ?? 0,
            DefensePower = m.Definition?.DefensePower ?? 0,
            Upkeep = m.Definition?.Upkeep ?? 0
        }).ToList();
    }

    public async Task<ServiceResult> RecruitUnitsAsync(int userId, RecruitUnitsDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Include(k => k.MilitaryUnits)
            .Include(k => k.Researches)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var unitDef = await _context.UnitDefinitions
            .FirstOrDefaultAsync(u => u.UnitType == dto.UnitType && u.Race == kingdom.Race);

        if (unitDef == null)
            return ServiceResult.Fail("Nieznany typ jednostki.");

        if (dto.Quantity <= 0)
            return ServiceResult.Fail("Nieprawidłowa ilość.");

        var (canRecruit, reason) = CheckCanRecruit(kingdom, unitDef);
        if (!canRecruit)
            return ServiceResult.Fail(reason!);

        // Sprawdź koszty
        long totalGold = (long)unitDef.CostGold * dto.Quantity;
        long totalIron = (long)unitDef.CostIron * dto.Quantity;
        long totalWood = (long)unitDef.CostWood * dto.Quantity;
        long totalFood = (long)unitDef.CostFood * dto.Quantity;

        if (kingdom.Gold < totalGold) return ServiceResult.Fail($"Za mało złota. Potrzeba: {totalGold}");
        if (kingdom.Iron < totalIron) return ServiceResult.Fail($"Za mało żelaza. Potrzeba: {totalIron}");
        if (kingdom.Wood < totalWood) return ServiceResult.Fail($"Za mało drewna. Potrzeba: {totalWood}");
        if (kingdom.Food < totalFood) return ServiceResult.Fail($"Za mało żywności. Potrzeba: {totalFood}");

        // Sprawdź populację (żołnierze biorą się z bezrobotnych)
        var unemployed = kingdom.Professions.FirstOrDefault(p => p.ProfessionType == "Unemployed");
        if (unemployed == null || unemployed.WorkerCount < dto.Quantity)
            return ServiceResult.Fail($"Za mało bezrobotnych do rekrutacji. Dostępnych: {unemployed?.WorkerCount ?? 0}");

        // Odejmij surowce
        kingdom.Gold -= totalGold;
        kingdom.Iron -= totalIron;
        kingdom.Wood -= totalWood;
        kingdom.Food -= totalFood;
        unemployed.WorkerCount -= dto.Quantity;

        // Znajdź lub utwórz rekord jednostki
        var militaryUnit = kingdom.MilitaryUnits.FirstOrDefault(m => m.UnitType == dto.UnitType);
        if (militaryUnit == null)
        {
            militaryUnit = new MilitaryUnit
            {
                KingdomId = kingdom.Id,
                UnitType = dto.UnitType,
                Quantity = 0,
                InTraining = 0
            };
            _context.MilitaryUnits.Add(militaryUnit);
        }

        if (unitDef.TrainingTime <= 1)
        {
            militaryUnit.Quantity += dto.Quantity;
        }
        else
        {
            militaryUnit.InTraining += dto.Quantity;
            militaryUnit.TrainingCompletesAt = DateTime.UtcNow.AddDays(unitDef.TrainingTime);
        }

        await _context.SaveChangesAsync();

        string msg = unitDef.TrainingTime <= 1
            ? $"Zrekrutowano {dto.Quantity}x {unitDef.DisplayName}."
            : $"Rozpoczęto szkolenie {dto.Quantity}x {unitDef.DisplayName}. Ukończenie za {unitDef.TrainingTime} dni.";

        return ServiceResult.Ok(msg);
    }

    private (bool canRecruit, string? reason) CheckCanRecruit(Kingdom kingdom, UnitDefinition unitDef)
    {
        // Sprawdź wymagany budynek
        var requiredBuilding = kingdom.Buildings
            .FirstOrDefault(b => b.BuildingType == unitDef.RequiredBuilding && b.Quantity > 0);

        if (requiredBuilding == null)
            return (false, $"Wymaga budynku: {unitDef.RequiredBuilding}");

        // Sprawdź wymaganą technologię
        if (!string.IsNullOrEmpty(unitDef.RequiredTech))
        {
            var requiredTech = kingdom.Researches?
                .FirstOrDefault(r => r.TechType == unitDef.RequiredTech && r.IsCompleted);

            if (requiredTech == null)
                return (false, $"Wymaga technologii: {unitDef.RequiredTech}");
        }

        return (true, null);
    }
}
