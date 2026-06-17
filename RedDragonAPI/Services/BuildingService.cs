using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
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

        // Rabat badań Inżynieria — wspólny dla wyceny i sprawdzenia możliwości budowy.
        decimal ecoDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "EcoBuildingCostReduction");

        return definitions.Select(d =>
        {
            var (canBuild, reason) = CheckCanBuild(kingdom, d, ecoDiscount);

            // Budynki gospodarcze: koszt skalowany ziemią (manual „Cena infrabudov").
            int costBudulec = d.CostBudulec;
            int costGold = d.CostGold;
            if (!d.IsSpecial)
            {
                (costBudulec, costGold) = ComputeEconomicCost(kingdom, ecoDiscount);
            }

            return new BuildingDefinitionDto
            {
                Id = d.Id,
                BuildingType = d.BuildingType,
                Category = d.Category,
                DisplayName = d.DisplayName,
                Description = d.Description,
                CostGold = costGold,
                CostBudulec = costBudulec,
                CostLand = d.CostLand,
                Row = d.Row,
                Col = d.Col,
                BaseCost = d.BaseCost,
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

        decimal ecoDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "EcoBuildingCostReduction");
        var (canBuild, reason) = CheckCanBuild(kingdom, definition, ecoDiscount);
        if (!canBuild)
            return ServiceResult.Fail(reason!);

        // === Budynek specjalny: koszt to budulec (BaseCost) gromadzony co turę; jeden naraz ===
        if (definition.IsSpecial)
        {
            var existing = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == dto.BuildingType);
            if (existing != null && existing.Quantity > 0)
                return ServiceResult.Fail("Można posiadać tylko jeden specjalny budynek tego typu.");

            // Tylko jeden budynek specjalny może być wznoszony jednocześnie.
            if (!string.IsNullOrEmpty(kingdom.CurrentSpecialBuilding))
            {
                var inProgress = await _context.BuildingDefinitions.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.BuildingType == kingdom.CurrentSpecialBuilding);
                return ServiceResult.Fail(
                    $"Już wznosisz budynek specjalny: {inProgress?.DisplayName ?? kingdom.CurrentSpecialBuilding}. Najpierw go dokończ.");
            }

            // Rabat badań: Architektura obniża koszt budulca budynków specjalnych.
            decimal specialDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "SpecialBuildingCostReduction");
            int budulecCost = Math.Max(1, (int)(definition.BaseCost * (1m - specialDiscount)));

            kingdom.CurrentSpecialBuilding = dto.BuildingType;
            kingdom.SpecialBuildingCost = budulecCost;
            kingdom.SpecialBuildingProgress = 0;

            // Rekord budynku „w budowie" — ResourceService dopełni go budulcem co turę.
            var special = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == dto.BuildingType);
            if (special == null)
            {
                special = new Building { KingdomId = kingdom.Id, BuildingType = dto.BuildingType, Quantity = 0, Level = 1 };
                _context.Buildings.Add(special);
            }
            special.IsUnderConstruction = true;

            await _context.SaveChangesAsync();
            return ServiceResult.Ok(
                $"Rozpoczęto wznoszenie: {definition.DisplayName}. Budulec ({budulecCost}) będzie gromadzony co turę.");
        }

        int quantity = dto.Quantity;

        // Red Dragon: koszt budynku gospodarczego (budulec + złoto) skaluje się z rozległością
        // królestwa wg wzoru z manuala; rabat badań Inżynieria (ecoDiscount) obniża koszt.
        var (budulecPerBuilding, goldPerBuilding) = ComputeEconomicCost(kingdom, ecoDiscount);
        long totalCostGold = (long)goldPerBuilding * quantity;
        int totalCostBudulec = budulecPerBuilding * quantity;
        int totalCostLand = definition.CostLand * quantity;

        if (kingdom.Gold < totalCostGold)
            return ServiceResult.Fail($"Za mało złota. Potrzeba: {totalCostGold}");
        if (kingdom.BudulecStored < totalCostBudulec)
            return ServiceResult.Fail($"Za mało budulca. Potrzeba: {totalCostBudulec}");
        if (kingdom.Land < totalCostLand)
            return ServiceResult.Fail($"Za mało ziemi. Potrzeba: {totalCostLand}");

        // Odejmij surowce
        kingdom.Gold -= totalCostGold;
        kingdom.BudulecStored -= totalCostBudulec;

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

    /// <summary>
    /// Koszt jednego budynku gospodarczego wg oryginalnego wzoru (manual „Cena infrabudov"):
    /// infrabody = int((149·ziemia/15000 + 1)·(1 − rabat)); dla ziemi > 20000 wariant rasowy
    /// (Olbrzym ÷2000, Człowiek ×1,5, pozostali ÷1000). Złoto = infrabody · 200.
    /// W startowym protektoracie obowiązuje 50% zniżki.
    /// </summary>
    public static (int budulec, int gold) ComputeEconomicCost(Kingdom kingdom, decimal ecoDiscount)
    {
        long land = kingdom.Land;
        decimal raw;
        if (land <= 20000)
            raw = 149m * land / 15000m + 1m;
        else if (kingdom.Race == "Olbrzym")
            raw = 181m + land / 2000m;
        else if (kingdom.Race == "Człowiek")
            raw = 1.5m * (181m + land / 1000m);
        else
            raw = 181m + land / 1000m;

        raw *= (1m - ecoDiscount);
        if (kingdom.IsProtected) raw *= 0.5m; // zniżka protektoratu startowego

        int budulec = Math.Max(1, (int)raw);
        int gold = budulec * 200;
        return (budulec, gold);
    }

    private (bool canBuild, string? reason) CheckCanBuild(Kingdom kingdom, BuildingDefinition definition, decimal ecoDiscount)
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
        if (definition.IsSpecial)
        {
            // Budynek specjalny: budulec gromadzony co turę, koszt złota = 0 — liczy się tylko ziemia.
            if (kingdom.Land < definition.CostLand) return (false, "Za mało ziemi");
        }
        else
        {
            var (budulec, gold) = ComputeEconomicCost(kingdom, ecoDiscount);
            if (kingdom.Gold < gold) return (false, "Za mało złota");
            if (kingdom.BudulecStored < budulec) return (false, "Za mało budulca");
            if (kingdom.Land < definition.CostLand) return (false, "Za mało ziemi");
        }

        return (true, null);
    }
}
