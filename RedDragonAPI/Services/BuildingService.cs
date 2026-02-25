using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class BuildingService : IBuildingService
{
    private readonly ApplicationDbContext _context;

    public BuildingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BuildingDefinitionDto>> GetAvailableBuildingsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Include(k => k.Researches)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return new List<BuildingDefinitionDto>();

        var definitions = await _context.BuildingDefinitions.ToListAsync();

        return definitions.Select(d =>
        {
            var (canBuild, reason) = CheckCanBuild(kingdom, d);
            return new BuildingDefinitionDto
            {
                Id = d.Id,
                BuildingType = d.BuildingType,
                Category = d.Category,
                DisplayName = d.DisplayName,
                Description = d.Description,
                CostGold = d.CostGold,
                CostWood = d.CostWood,
                CostStone = d.CostStone,
                CostIron = d.CostIron,
                CostMana = d.CostMana,
                CostLand = d.CostLand,
                BuildTime = d.BuildTime,
                RequiredBuildingType = d.RequiredBuildingType,
                RequiredTechnology = d.RequiredTechnology,
                IsSpecial = d.IsSpecial,
                BonusTurnsPerDay = d.BonusTurnsPerDay,
                ProductionBonus = d.ProductionBonus,
                DefenseBonus = d.DefenseBonus,
                PopulationCapacity = d.PopulationCapacity,
                CanBuild = canBuild,
                CannotBuildReason = reason
            };
        }).ToList();
    }

    public async Task<List<BuildingDto>> GetMyBuildingsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return new List<BuildingDto>();

        return kingdom.Buildings.Select(b => new BuildingDto
        {
            Id = b.Id,
            BuildingType = b.BuildingType,
            DisplayName = b.Definition?.DisplayName ?? b.BuildingType,
            Category = b.Definition?.Category ?? "",
            Description = b.Definition?.Description,
            Quantity = b.Quantity,
            Level = b.Level,
            IsUnderConstruction = b.IsUnderConstruction,
            ConstructionCompletesAt = b.ConstructionCompletesAt
        }).ToList();
    }

    public async Task<ServiceResult> ConstructBuildingAsync(int userId, ConstructBuildingDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Include(k => k.Researches)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var definition = await _context.BuildingDefinitions
            .FirstOrDefaultAsync(d => d.BuildingType == dto.BuildingType);

        if (definition == null)
            return ServiceResult.Fail("Nieznany typ budynku.");

        var (canBuild, reason) = CheckCanBuild(kingdom, definition);
        if (!canBuild)
            return ServiceResult.Fail(reason!);

        // Sprawdź czy specjalny budynek już istnieje
        if (definition.IsSpecial)
        {
            var existing = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == dto.BuildingType);
            if (existing != null && existing.Quantity > 0)
                return ServiceResult.Fail("Można posiadać tylko jeden specjalny budynek tego typu.");
        }

        int quantity = definition.IsSpecial ? 1 : dto.Quantity;

        // Pobierz koszty
        long totalCostGold = (long)definition.CostGold * quantity;
        long totalCostWood = (long)definition.CostWood * quantity;
        long totalCostStone = (long)definition.CostStone * quantity;
        long totalCostIron = (long)definition.CostIron * quantity;
        long totalCostMana = (long)definition.CostMana * quantity;
        int totalCostLand = definition.CostLand * quantity;

        if (kingdom.Gold < totalCostGold)
            return ServiceResult.Fail($"Za mało złota. Potrzeba: {totalCostGold}");
        if (kingdom.Wood < totalCostWood)
            return ServiceResult.Fail($"Za mało drewna. Potrzeba: {totalCostWood}");
        if (kingdom.Stone < totalCostStone)
            return ServiceResult.Fail($"Za mało kamienia. Potrzeba: {totalCostStone}");
        if (kingdom.Iron < totalCostIron)
            return ServiceResult.Fail($"Za mało żelaza. Potrzeba: {totalCostIron}");
        if (kingdom.Mana < totalCostMana)
            return ServiceResult.Fail($"Za mało many. Potrzeba: {totalCostMana}");
        if (kingdom.Land < totalCostLand)
            return ServiceResult.Fail($"Za mało ziemi. Potrzeba: {totalCostLand}");

        // Odejmij surowce
        kingdom.Gold -= totalCostGold;
        kingdom.Wood -= totalCostWood;
        kingdom.Stone -= totalCostStone;
        kingdom.Iron -= totalCostIron;
        kingdom.Mana -= totalCostMana;

        // Znajdź lub utwórz rekord budynku
        var building = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == dto.BuildingType);
        if (building == null)
        {
            building = new Building
            {
                KingdomId = kingdom.Id,
                BuildingType = dto.BuildingType,
                Quantity = 0,
                Level = 1
            };
            _context.Buildings.Add(building);
        }

        if (definition.BuildTime <= 1)
        {
            // Budowa natychmiastowa
            building.Quantity += quantity;
        }
        else
        {
            // Kolejkuj budowę
            building.IsUnderConstruction = true;
            building.ConstructionCompletesAt = DateTime.UtcNow.AddDays(definition.BuildTime);

            _context.QueuedActions.Add(new QueuedAction
            {
                KingdomId = kingdom.Id,
                ActionType = "Construction",
                ActionData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    BuildingType = dto.BuildingType,
                    Quantity = quantity
                }),
                ScheduledFor = DateTime.UtcNow.AddDays(definition.BuildTime),
                Status = "Pending"
            });
        }

        await _context.SaveChangesAsync();

        string msg = definition.BuildTime <= 1
            ? $"Zbudowano {quantity}x {definition.DisplayName}."
            : $"Rozpoczęto budowę {quantity}x {definition.DisplayName}. Ukończenie za {definition.BuildTime} dni.";

        return ServiceResult.Ok(msg);
    }

    private (bool canBuild, string? reason) CheckCanBuild(Kingdom kingdom, BuildingDefinition definition)
    {
        // Sprawdź wymagany budynek
        if (!string.IsNullOrEmpty(definition.RequiredBuildingType))
        {
            var required = kingdom.Buildings
                .FirstOrDefault(b => b.BuildingType == definition.RequiredBuildingType && b.Quantity > 0);

            if (required == null)
                return (false, $"Wymaga budynku: {definition.RequiredBuildingType}");
        }

        // Sprawdź wymaganą technologię
        if (!string.IsNullOrEmpty(definition.RequiredTechnology))
        {
            var required = kingdom.Researches?
                .FirstOrDefault(r => r.TechType == definition.RequiredTechnology && r.IsCompleted);

            if (required == null)
                return (false, $"Wymaga technologii: {definition.RequiredTechnology}");
        }

        // Sprawdź zasoby
        if (kingdom.Gold < definition.CostGold) return (false, "Za mało złota");
        if (kingdom.Wood < definition.CostWood) return (false, "Za mało drewna");
        if (kingdom.Stone < definition.CostStone) return (false, "Za mało kamienia");
        if (kingdom.Iron < definition.CostIron) return (false, "Za mało żelaza");
        if (kingdom.Mana < definition.CostMana) return (false, "Za mało many");
        if (kingdom.Land < definition.CostLand) return (false, "Za mało ziemi");

        return (true, null);
    }
}
