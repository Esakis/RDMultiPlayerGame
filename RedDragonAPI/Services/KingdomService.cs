using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
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

    // Rasy oryginalnego Red Dragon: 10 z reddragon.cz + Gnom i Br-Oug
    // z polskiego serwera reddragon.pl (definicje: RaceDefinitions, docs/MECHANIKA.md)
    public static readonly HashSet<string> AllRaces = new()
    {
        "Człowiek", "Elf", "Krasnolud", "Hobbit", "Nekromant",
        "Dżin", "Goblin", "Ent", "Wampir", "Olbrzym", "Gnom", "Br-Oug"
    };

    public async Task<Kingdom> CreateKingdomAsync(int userId, string name, string race, int eraId)
    {
        if (!AllRaces.Contains(race))
            race = "Człowiek";

        var raceDef = await _context.RaceDefinitions.FirstOrDefaultAsync(r => r.Name == race);
        int turnsPerDay = raceDef?.TurnsPerDay ?? 15;

        var kingdom = new Kingdom
        {
            UserId = userId,
            Name = name,
            Race = race,
            IsMagicRace = (raceDef?.MagicBooks ?? 0) > 0,
            EraId = eraId,
            Land = 100,
            Gold = 50000,
            Food = 10000,
            Stone = 2000,
            Budulec = 0,
            BudulecStored = 0,
            Weapons = 0,
            Mana = 0,
            Population = 1000,
            Popularity = 100,
            Wages = 50,
            Education = 0,
            TurnsAvailable = turnsPerDay,
            TurnsPerDay = turnsPerDay,
            // „trojtah" — maksymalnie potrójny dzienny przydział (+4, by standard dawał 49)
            MaxTurns = turnsPerDay * 3 + 4,
            TurnNumber = 0,
            Age = 0,
            IsProtected = true,
            ProtectionDaysLeft = 5
        };

        _context.Kingdoms.Add(kingdom);
        await _context.SaveChangesAsync();

        // Profesje oryginalnego RD: bezrobotni + farmerzy, kamieniarze, murarze,
        // kupcy, alchemicy, płatnerze, druidzi, magowie, naukowcy
        var professionTypes = new[]
        {
            "Bezrobotni", "Alchemicy", "Chłopi", "Druidzi",
            "Kamieniarze", "Murarze", "Płatnerze", "Kupcy", "Magowie", "Naukowcy"
        };

        foreach (var profType in professionTypes)
        {
            _context.Professions.Add(new Profession
            {
                KingdomId = kingdom.Id,
                ProfessionType = profType,
                WorkerCount = profType == "Bezrobotni" ? 1000 : 0,
                NoviceCount = 0,
                MaxCapacity = 0,
                ProductionPerTurn = 0,
                NovicePercent = 0
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
            .FirstOrDefault(p => p.ProfessionType == "Bezrobotni");

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

        // Oryginalny wzór: cena = ((z+x)^3,5 − z^3,5) / 600 000
        long cost = CalculateLandCost(kingdom.Land, amount);
        // Rabat badań: Rekultywacja/Osadnictwo (LandCostReduction)
        decimal landDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "LandCostReduction");
        cost = (long)(cost * (1m - landDiscount));
        if (kingdom.Gold < cost)
            return ServiceResult.Fail($"Za mało złota. Potrzeba: {cost}, posiadasz: {kingdom.Gold}");

        kingdom.Gold -= cost;
        kingdom.Land += amount;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zakupiono {amount} akrów za {cost} złota.");
    }

    public async Task<ServiceResult> FreezeAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.IsFrozen) return ServiceResult.Fail("Księstwo jest już zamrożone.");

        // Nie można zamrozić, gdy są zaplanowane ataki na Twoje księstwo (zapobiega unikom)
        bool incoming = await _context.QueuedActions.AnyAsync(q =>
            q.TargetKingdomId == kingdom.Id && q.Status == "Pending" && q.ActionType == "MilitaryAttack");
        if (incoming)
            return ServiceResult.Fail("Nie możesz zamrozić księstwa — masz zaplanowane ataki wymierzone w Ciebie.");

        kingdom.IsFrozen = true;
        kingdom.FrozenAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Księstwo zamrożone. Przeliczenia i ataki są wstrzymane do odmrożenia.");
    }

    public async Task<ServiceResult> UnfreezeAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (!kingdom.IsFrozen) return ServiceResult.Fail("Księstwo nie jest zamrożone.");

        kingdom.IsFrozen = false;
        kingdom.FrozenAt = null;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Księstwo odmrożone — możesz znów działać.");
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
                CoalitionTag = k.Coalition != null ? k.Coalition.Tag : null,
                IsProtected = k.IsProtected,
                IsFrozen = k.IsFrozen
            })
            .ToListAsync();
    }

    public static long CalculateLandCost(int currentLand, int amount)
    {
        double z = currentLand;
        double cost = (Math.Pow(z + amount, 3.5) - Math.Pow(z, 3.5)) / 600_000d;
        return Math.Max(1, (long)Math.Ceiling(cost));
    }

    private static KingdomDto MapToDto(Kingdom kingdom)
    {
        return new KingdomDto
        {
            Id = kingdom.Id,
            Name = kingdom.Name,
            Race = kingdom.Race,
            IsMagicRace = kingdom.IsMagicRace,
            Land = kingdom.Land,
            Gold = kingdom.Gold,
            Food = kingdom.Food,
            Stone = kingdom.Stone,
            Budulec = kingdom.Budulec,
            BudulecStored = kingdom.BudulecStored,
            Weapons = kingdom.Weapons,
            Mana = kingdom.Mana,
            Bodies = kingdom.Bodies,
            Population = kingdom.Population,
            Popularity = kingdom.Popularity,
            Wages = kingdom.Wages,
            Education = kingdom.Education,
            TurnsAvailable = kingdom.TurnsAvailable,
            TurnsPerDay = kingdom.TurnsPerDay,
            MaxTurns = kingdom.MaxTurns,
            TurnNumber = kingdom.TurnNumber,
            Age = kingdom.Age,
            CurrentSpecialBuilding = kingdom.CurrentSpecialBuilding,
            SpecialBuildingProgress = kingdom.SpecialBuildingProgress,
            SpecialBuildingCost = kingdom.SpecialBuildingCost,
            CoalitionId = kingdom.CoalitionId,
            CoalitionName = kingdom.Coalition?.Name,
            CoalitionRole = kingdom.CoalitionRole,
            EraId = kingdom.EraId,
            EraName = kingdom.Era?.Name,
            IsProtected = kingdom.IsProtected,
            ProtectionDaysLeft = kingdom.ProtectionDaysLeft,
            IsFrozen = kingdom.IsFrozen,
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
                DisplayName = p.ProfessionType,
                WorkerCount = p.WorkerCount,
                NoviceCount = p.NoviceCount,
                MaxCapacity = p.MaxCapacity,
                ProductionPerTurn = p.ProductionPerTurn,
                NovicePercent = p.NovicePercent
            }).ToList()
        };
    }
}
