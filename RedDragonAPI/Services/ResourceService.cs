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

        // Produkcja z budynków
        foreach (var building in kingdom.Buildings.Where(b => !b.IsUnderConstruction && b.Quantity > 0))
        {
            switch (building.BuildingType)
            {
                case "Farm":
                    kingdom.Food += building.Quantity * 500;
                    break;
                case "Sawmill":
                    kingdom.Wood += building.Quantity * 300;
                    break;
                case "Mine":
                    kingdom.Iron += building.Quantity * 200;
                    break;
            }
        }

        // Produkcja z zawodów
        foreach (var prof in kingdom.Professions)
        {
            switch (prof.ProfessionType)
            {
                case "Stonemason":
                    var quarries = kingdom.Buildings
                        .FirstOrDefault(b => b.BuildingType == "Quarry" && b.Quantity > 0);
                    if (quarries != null)
                        kingdom.Stone += prof.WorkerCount * 100L;
                    break;
                case "Merchant":
                    kingdom.Gold += prof.WorkerCount * 10L;
                    break;
                case "Alchemist":
                    kingdom.Gold += prof.WorkerCount * 20L;
                    break;
                case "Druid":
                    kingdom.Mana += prof.WorkerCount * 5L;
                    break;
            }
        }

        // Bonusy z warsztatów i manufaktur
        decimal productionBonus = 0;
        foreach (var building in kingdom.Buildings.Where(b => !b.IsUnderConstruction && b.Quantity > 0))
        {
            if (building.Definition != null && building.Definition.ProductionBonus > 0)
            {
                productionBonus += building.Definition.ProductionBonus * building.Quantity;
            }
        }

        if (productionBonus > 0)
        {
            // Zastosuj bonus do wszystkich surowców produkowanych w tej turze
            // (uproszczone - bonus do bazowej produkcji)
        }

        // Przyrost ludności
        kingdom.Population += kingdom.PopulationGrowthRate;

        // Sprawdź limit populacji z domów
        int populationCap = 1000; // bazowy
        var houses = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "House");
        if (houses != null && houses.Definition != null)
        {
            populationCap += houses.Quantity * houses.Definition.PopulationCapacity;
        }

        if (kingdom.Population > populationCap)
        {
            kingdom.Population = populationCap;
        }

        // Upkeep armii (żywność)
        var militaryUnits = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id)
            .Include(m => m.Definition)
            .ToListAsync();

        long totalUpkeep = militaryUnits.Sum(m => (long)m.Quantity * (m.Definition?.Upkeep ?? 0));
        kingdom.Food -= totalUpkeep;

        if (kingdom.Food < 0)
        {
            kingdom.Food = 0;
            // Głód - jednostki dezertują
            foreach (var unit in militaryUnits)
            {
                int desertions = (int)(unit.Quantity * 0.05); // 5% dezercji
                unit.Quantity = Math.Max(0, unit.Quantity - desertions);
            }
        }
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
