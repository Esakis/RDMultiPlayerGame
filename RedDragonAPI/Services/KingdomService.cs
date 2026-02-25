using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class KingdomService : IKingdomService
{
    private readonly ApplicationDbContext _context;

    public KingdomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KingdomDto?> GetKingdomByUserIdAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Coalition)
            .Include(k => k.Era)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null) return null;

        return MapToDto(kingdom);
    }

    public async Task<KingdomDto?> GetKingdomByIdAsync(int kingdomId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Coalition)
            .Include(k => k.Era)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.Id == kingdomId);

        if (kingdom == null) return null;

        return MapToDto(kingdom);
    }

    public async Task<Kingdom> CreateKingdomAsync(int userId, string name, int eraId)
    {
        var kingdom = new Kingdom
        {
            UserId = userId,
            Name = name,
            Race = "Human",
            EraId = eraId,
            Land = 100,
            Gold = 50000,
            Food = 10000,
            Wood = 5000,
            Stone = 2000,
            Iron = 1000,
            Mana = 500,
            Population = 1000,
            PopulationGrowthRate = 50,
            TurnsAvailable = 15,
            TurnsPerDay = 15,
            MaxTurns = 49,
            Age = 0
        };

        _context.Kingdoms.Add(kingdom);
        await _context.SaveChangesAsync();

        // Utwórz domyślne profesje
        var professionTypes = new[]
        {
            "Unemployed", "Stonemason", "Builder", "Merchant",
            "Alchemist", "Druid", "Mage", "Scientist", "Soldier"
        };

        foreach (var profType in professionTypes)
        {
            _context.Professions.Add(new Profession
            {
                KingdomId = kingdom.Id,
                ProfessionType = profType,
                WorkerCount = profType == "Unemployed" ? 1000 : 0,
                ProductionPerTurn = 0
            });
        }

        await _context.SaveChangesAsync();

        return kingdom;
    }

    public async Task<ServiceResult> AssignWorkersAsync(int userId, AssignWorkersDto dto)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var targetProfession = kingdom.Professions
            .FirstOrDefault(p => p.ProfessionType == dto.ProfessionType);

        if (targetProfession == null)
            return ServiceResult.Fail("Nieznany typ profesji.");

        var unemployed = kingdom.Professions
            .FirstOrDefault(p => p.ProfessionType == "Unemployed");

        if (unemployed == null)
            return ServiceResult.Fail("Brak danych o bezrobotnych.");

        if (dto.WorkerCount > 0)
        {
            // Przydzielanie pracowników
            if (unemployed.WorkerCount < dto.WorkerCount)
                return ServiceResult.Fail($"Za mało bezrobotnych. Dostępnych: {unemployed.WorkerCount}");

            unemployed.WorkerCount -= dto.WorkerCount;
            targetProfession.WorkerCount += dto.WorkerCount;
        }
        else
        {
            // Zwalnianie pracowników (ujemna wartość)
            int toFree = Math.Abs(dto.WorkerCount);
            if (targetProfession.WorkerCount < toFree)
                return ServiceResult.Fail($"Za mało pracowników w tej profesji. Aktualnie: {targetProfession.WorkerCount}");

            targetProfession.WorkerCount -= toFree;
            unemployed.WorkerCount += toFree;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Pracownicy zostali przydzieleni.");
    }

    public async Task<ServiceResult> BuyLandAsync(int userId, int amount)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        if (amount <= 0)
            return ServiceResult.Fail("Nieprawidłowa ilość.");

        long cost = amount * 500L; // 500 złota za akr
        if (kingdom.Gold < cost)
            return ServiceResult.Fail($"Za mało złota. Potrzeba: {cost}, posiadasz: {kingdom.Gold}");

        kingdom.Gold -= cost;
        kingdom.Land += amount;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zakupiono {amount} akrów za {cost} złota.");
    }

    public async Task<List<KingdomSummaryDto>> GetAllKingdomsAsync(int eraId)
    {
        return await _context.Kingdoms
            .Where(k => k.EraId == eraId)
            .Include(k => k.Coalition)
            .Select(k => new KingdomSummaryDto
            {
                Id = k.Id,
                Name = k.Name,
                Race = k.Race,
                Land = k.Land,
                Population = k.Population,
                CoalitionTag = k.Coalition != null ? k.Coalition.Tag : null
            })
            .ToListAsync();
    }

    private static KingdomDto MapToDto(Kingdom kingdom)
    {
        return new KingdomDto
        {
            Id = kingdom.Id,
            Name = kingdom.Name,
            Race = kingdom.Race,
            Land = kingdom.Land,
            Gold = kingdom.Gold,
            Food = kingdom.Food,
            Wood = kingdom.Wood,
            Stone = kingdom.Stone,
            Iron = kingdom.Iron,
            Mana = kingdom.Mana,
            Population = kingdom.Population,
            PopulationGrowthRate = kingdom.PopulationGrowthRate,
            TurnsAvailable = kingdom.TurnsAvailable,
            TurnsPerDay = kingdom.TurnsPerDay,
            MaxTurns = kingdom.MaxTurns,
            Age = kingdom.Age,
            CoalitionId = kingdom.CoalitionId,
            CoalitionName = kingdom.Coalition?.Name,
            CoalitionRole = kingdom.CoalitionRole,
            EraId = kingdom.EraId,
            EraName = kingdom.Era?.Name,
            Buildings = kingdom.Buildings.Select(b => new BuildingDto
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
            }).ToList(),
            MilitaryUnits = kingdom.MilitaryUnits.Select(m => new MilitaryUnitDto
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
            }).ToList(),
            Professions = kingdom.Professions.Select(p => new ProfessionDto
            {
                ProfessionType = p.ProfessionType,
                DisplayName = GetProfessionDisplayName(p.ProfessionType),
                WorkerCount = p.WorkerCount,
                ProductionPerTurn = p.ProductionPerTurn
            }).ToList()
        };
    }

    private static string GetProfessionDisplayName(string profType) => profType switch
    {
        "Unemployed" => "Bezrobotni",
        "Stonemason" => "Kamieniarze",
        "Builder" => "Murarze",
        "Merchant" => "Kupcy",
        "Alchemist" => "Alchemicy",
        "Druid" => "Druidzi",
        "Mage" => "Magowie",
        "Scientist" => "Naukowcy",
        "Soldier" => "Żołnierze",
        _ => profType
    };
}
