using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class ResourceService : IResourceService
{
    private readonly ApplicationDbContext _context;

    public ResourceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task GenerateResourcesForKingdomAsync(Kingdom kingdom)
    {
        // Załaduj relacje jeśli nie załadowane
        if (kingdom.Buildings == null || !kingdom.Buildings.Any())
        {
            await _context.Entry(kingdom)
                .Collection(k => k.Buildings)
                .Query()
                .Include(b => b.Definition)
                .LoadAsync();
        }

        if (kingdom.Professions == null || !kingdom.Professions.Any())
        {
            await _context.Entry(kingdom)
                .Collection(k => k.Professions)
                .LoadAsync();
        }

        // === Red Dragon Turn Processing ===
        // Edukacja bonus from Naukowcy (max 15%)
        decimal educationBonus = 1m + (kingdom.Education / 100m);

        // Produkcja z zawodów
        foreach (var prof in kingdom.Professions)
        {
            // Effective workers = total - novices*0.9 (novices produce at 10%)
            int effectiveWorkers = prof.WorkerCount - (int)(prof.NoviceCount * 0.9m);
            long production = 0;

            switch (prof.ProfessionType)
            {
                case "Alchemicy":
                    production = (long)(effectiveWorkers * 10L * educationBonus);
                    kingdom.Gold += production;
                    break;
                case "Chłopi":
                    production = (long)(effectiveWorkers * 5L * educationBonus);
                    kingdom.Food += production;
                    break;
                case "Druidzi":
                    production = (long)(effectiveWorkers * 1L * educationBonus);
                    kingdom.Mana += production;
                    break;
                case "Kamieniarze":
                    production = (long)(effectiveWorkers * 5L * educationBonus);
                    kingdom.Stone += production;
                    break;
                case "Murarze":
                    // Murarze need kamienie to produce budulec
                    long stoneNeeded = effectiveWorkers * 2L;
                    long stoneUsed = Math.Min(stoneNeeded, kingdom.Stone);
                    production = (long)(stoneUsed / 2m * educationBonus);
                    kingdom.Stone -= stoneUsed;
                    kingdom.Budulec += production;
                    break;
                case "Płatnerze":
                    production = (long)(effectiveWorkers * 3L * educationBonus);
                    kingdom.Weapons += production;
                    break;
                case "Kupcy":
                    production = (long)(effectiveWorkers * 1000L * educationBonus);
                    kingdom.Gold += production;
                    break;
            }

            prof.ProductionPerTurn = production;
        }

        // Produkcja z manufaktur (buildings that auto-produce)
        foreach (var building in kingdom.Buildings.Where(b => !b.IsUnderConstruction && b.Quantity > 0))
        {
            if (building.Definition != null && building.Definition.ProductionBonus > 0)
            {
                // Manufaktury add production bonus
            }
        }

        // Pensje (wages) - all workers * wages
        int totalWorkers = kingdom.Professions.Where(p => p.ProfessionType != "Bezrobotni").Sum(p => p.WorkerCount);
        long wagesCost = (long)totalWorkers * kingdom.Wages;
        kingdom.Gold -= wagesCost;

        // Żołd (army pay) = soldiers * wages * 2
        var militaryUnits = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id)
            .Include(m => m.Definition)
            .ToListAsync();

        int totalSoldiers = militaryUnits.Sum(m => m.Quantity);
        long armyPay = (long)totalSoldiers * kingdom.Wages * 2;
        kingdom.Gold -= armyPay;

        // Jedzenie - 1 per person + 1 per soldier
        long foodNeeded = kingdom.Population + totalSoldiers;
        kingdom.Food -= foodNeeded;

        if (kingdom.Food < 0)
        {
            kingdom.Food = 0;
            // Głód - ludzie uciekają, popularność spada
            kingdom.Popularity = Math.Max(0, kingdom.Popularity - 10);
            int fleeing = (int)(kingdom.Population * 0.05);
            kingdom.Population = Math.Max(100, kingdom.Population - fleeing);
        }

        // Budulec split: special building first, then storage
        if (!string.IsNullOrEmpty(kingdom.CurrentSpecialBuilding) && kingdom.Budulec > 0)
        {
            int needed = kingdom.SpecialBuildingCost - kingdom.SpecialBuildingProgress;
            int used = (int)Math.Min(kingdom.Budulec, needed);
            kingdom.SpecialBuildingProgress += used;
            kingdom.Budulec -= used;

            if (kingdom.SpecialBuildingProgress >= kingdom.SpecialBuildingCost)
            {
                // Complete the special building
                var building = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == kingdom.CurrentSpecialBuilding);
                if (building != null)
                {
                    building.Quantity = 1;
                    building.IsUnderConstruction = false;
                }
                kingdom.CurrentSpecialBuilding = null;
                kingdom.SpecialBuildingProgress = 0;
                kingdom.SpecialBuildingCost = 0;
            }
        }

        // Move remaining budulec to storage (limit: 7500 + land/4)
        long budulecLimit = 7500 + kingdom.Land / 4;
        kingdom.BudulecStored = Math.Min(kingdom.BudulecStored + kingdom.Budulec, budulecLimit);
        kingdom.Budulec = 0;

        // Popularność from wages
        int idealWage = 50; // 42 with Zajazd u Czerwonego Smoka
        bool hasZajazd = kingdom.Buildings.Any(b => b.BuildingType == "ZajazdCzerwonego" && b.Quantity > 0);
        if (hasZajazd) idealWage = 42;

        if (kingdom.Wages >= idealWage)
            kingdom.Popularity = Math.Min(100, kingdom.Popularity + 1);
        else
            kingdom.Popularity = Math.Max(0, kingdom.Popularity - 1);

        // No gold = popularity drops fast
        if (kingdom.Gold < 0)
        {
            kingdom.Popularity = Math.Max(0, kingdom.Popularity - 5);
            kingdom.Gold = 0;
        }

        // Population migration based on popularity
        if (kingdom.Popularity >= 80)
        {
            int immigrants = (int)(kingdom.Population * 0.02);
            kingdom.Population += immigrants;
        }
        else if (kingdom.Popularity < 50)
        {
            int emigrants = (int)(kingdom.Population * 0.03);
            kingdom.Population = Math.Max(100, kingdom.Population - emigrants);
        }

        // Population cap from houses
        int populationCap = 1000;
        var houses = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "Domy");
        if (houses != null && houses.Definition != null)
        {
            populationCap += houses.Quantity * houses.Definition.PopulationCapacity;
        }
        if (kingdom.Population > populationCap)
            kingdom.Population = populationCap;

        // Novice training (5% of novices become skilled per turn)
        foreach (var prof in kingdom.Professions.Where(p => p.NoviceCount > 0))
        {
            int trained = Math.Max(1, (int)(prof.NoviceCount * 0.05));
            prof.NoviceCount = Math.Max(0, prof.NoviceCount - trained);
            prof.NovicePercent = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100
                : 0;
        }

        // Mana sold at turn end (except Elves)
        if (kingdom.Race != "Elfy")
        {
            long manaValue = kingdom.Mana * 5;
            kingdom.Gold += manaValue;
            kingdom.Mana = 0;
        }

        kingdom.TurnNumber++;
    }

    public async Task GenerateResourcesForAllAsync()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .Where(k => k.Era.IsActive)
            .ToListAsync();

        foreach (var kingdom in kingdoms)
        {
            await GenerateResourcesForKingdomAsync(kingdom);
        }

        await _context.SaveChangesAsync();
    }
}
