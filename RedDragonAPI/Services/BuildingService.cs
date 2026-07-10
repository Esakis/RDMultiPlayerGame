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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null) return new List<BuildingDefinitionDto>();

        var definitions = await _context.BuildingDefinitions.ToListAsync();

        // Rabat badań Inżynieria — wspólny dla wyceny i sprawdzenia możliwości budowy.
        decimal ecoDiscount = await EcoDiscountAsync(kingdom);

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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var definition = await _context.BuildingDefinitions
            .FirstOrDefaultAsync(d => d.BuildingType == dto.BuildingType);

        if (definition == null)
            return ServiceResult.Fail("Nieznany typ budynku.");

        decimal ecoDiscount = await EcoDiscountAsync(kingdom);
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

            int budulecCost = await ComputeSpecialBudulecCostAsync(kingdom, definition);

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
        // Rasy (§2.2): Człowiek — budynki infrastrukturalne o 10% tańsze (złoto i budulec);
        // Br-Oug — budynki o 50% droższe.
        if (kingdom.Race == "Człowiek")
        {
            totalCostGold = (long)(totalCostGold * 0.9m);
            totalCostBudulec = (int)(totalCostBudulec * 0.9m);
        }
        if (kingdom.Race == "Br-Oug")
        {
            totalCostGold = (long)(totalCostGold * 1.5m);
            totalCostBudulec = (int)(totalCostBudulec * 1.5m);
        }
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

    public async Task<ServiceResult> DemolishBuildingAsync(int userId, ConstructBuildingDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var definition = await _context.BuildingDefinitions
            .FirstOrDefaultAsync(d => d.BuildingType == dto.BuildingType);
        if (definition == null)
            return ServiceResult.Fail("Nieznany typ budynku.");

        var building = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == dto.BuildingType);
        if (building == null || building.Quantity <= 0)
            return ServiceResult.Fail("Nie posiadasz tego budynku.");

        int quantity = Math.Min(Math.Max(1, dto.Quantity), building.Quantity);
        building.Quantity -= quantity;

        // Inżynieria 5 (Dracopedia): wyburzanie budynków zwraca 80% budulca.
        // U Ożywieńców (Nekromant) poziom działa zamiast tego jako rabat 32%.
        long refund = 0;
        if (kingdom.Race != "Nekromant" && await HasTechAsync(kingdom.Id, "Inzynieria5"))
        {
            if (definition.IsSpecial)
            {
                refund = (long)(await ComputeSpecialBudulecCostAsync(kingdom, definition) * 0.8m);
            }
            else
            {
                var (budulecPer, _) = ComputeEconomicCost(kingdom, await EcoDiscountAsync(kingdom));
                decimal raceMult = kingdom.Race == "Człowiek" ? 0.9m
                    : kingdom.Race == "Br-Oug" ? 1.5m : 1m;
                refund = (long)(budulecPer * raceMult * quantity * 0.8m);
            }
            kingdom.BudulecStored += refund;
        }

        // Budynek bez sztuk i bez trwającej budowy można usunąć z bazy (zwalnia drzewko specjalne).
        if (building.Quantity == 0 && !building.IsUnderConstruction)
            _context.Buildings.Remove(building);

        await _context.SaveChangesAsync();
        string refundInfo = refund > 0 ? $" Odzyskano {refund} budulca (Inżynieria nowoczesna)." : "";
        return ServiceResult.Ok($"Wyburzono {quantity}x {definition.DisplayName}. Zajmowana ziemia została zwolniona.{refundInfo}");
    }

    /// <summary>Ukończone badanie danego typu.</summary>
    private Task<bool> HasTechAsync(int kingdomId, string techType) =>
        _context.Researches.AnyAsync(r => r.KingdomId == kingdomId && r.TechType == techType && r.IsCompleted);

    /// <summary>
    /// Rabat Inżynierii na zabudowania gospodarcze. Wyjątki (Dracopedia):
    /// Elf z Inżynierią 4 oraz Ożywieniec (Nekromant) z Inżynierią 5 mają 32%
    /// zamiast efektów specjalnych tych poziomów.
    /// </summary>
    private async Task<decimal> EcoDiscountAsync(Kingdom kingdom)
    {
        decimal d = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "EcoBuildingCostReduction");
        if (kingdom.Race == "Elf" && await HasTechAsync(kingdom.Id, "Inzynieria4")) d = Math.Max(d, 0.32m);
        if (kingdom.Race == "Nekromant" && await HasTechAsync(kingdom.Id, "Inzynieria5")) d = Math.Max(d, 0.32m);
        return d;
    }

    /// <summary>
    /// Koszt budulca budynku specjalnego z rabatami: Architektura (u Enta poz. 4/5
    /// dają 21%/30% zamiast efektów specjalnych), rasa, protektorat. Architektura 5:
    /// budynki 6. i 7. rzędu budują się o turę szybciej — przy stałej produkcji
    /// budulca odpowiada to kosztowi ×(t−1)/t.
    /// </summary>
    private async Task<int> ComputeSpecialBudulecCostAsync(Kingdom kingdom, BuildingDefinition definition)
    {
        decimal specialDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "SpecialBuildingCostReduction");
        if (kingdom.Race == "Ent")
        {
            if (await HasTechAsync(kingdom.Id, "Architektura5")) specialDiscount = Math.Max(specialDiscount, 0.30m);
            else if (await HasTechAsync(kingdom.Id, "Architektura4")) specialDiscount = Math.Max(specialDiscount, 0.21m);
        }
        decimal specialCost = definition.BaseCost * (1m - specialDiscount);
        // Rasy (§2.2): Krasnolud — budynki specjalne o 10% tańsze; Br-Oug — o 50% droższe.
        if (kingdom.Race == "Krasnolud") specialCost *= 0.9m;
        if (kingdom.Race == "Br-Oug") specialCost *= 1.5m;
        // Protektorat początkowy (Dracopedia §1): budynki specjalne −60%.
        if (kingdom.IsProtected) specialCost *= 0.4m;
        if (definition.Row >= 6 && definition.BuildTime > 1
            && await HasTechAsync(kingdom.Id, "Architektura5"))
            specialCost *= (definition.BuildTime - 1m) / definition.BuildTime;
        return Math.Max(1, (int)specialCost);
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
