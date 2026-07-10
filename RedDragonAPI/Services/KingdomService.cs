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
            .Include(k => k.ActiveSpells).ThenInclude(s => s.Spell)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null) return null;

        int pendingGenerals = await _context.Generals
            .CountAsync(g => g.KingdomId == kingdom.Id && g.IsPending);
        var recentEvents = await GetRecentEventsAsync(kingdom.Id);
        var research = await GetCurrentResearchInfoAsync(kingdom);

        return MapToDto(kingdom, pendingGenerals, recentEvents, research);
    }

    public async Task<KingdomDto?> GetKingdomByIdAsync(int kingdomId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Coalition)
            .Include(k => k.Era)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .Include(k => k.ActiveSpells).ThenInclude(s => s.Spell)
            .FirstOrDefaultAsync(k => k.Id == kingdomId);

        if (kingdom == null) return null;

        int pendingGenerals = await _context.Generals
            .CountAsync(g => g.KingdomId == kingdom.Id && g.IsPending);
        var recentEvents = await GetRecentEventsAsync(kingdom.Id);
        var research = await GetCurrentResearchInfoAsync(kingdom);

        return MapToDto(kingdom, pendingGenerals, recentEvents, research);
    }

    /// <summary>Aktualnie rozwijana dziedzina nauki wraz z postępem (docs/MECHANIKA.md §13).</summary>
    private async Task<(string? Name, long Progress, long Cost)> GetCurrentResearchInfoAsync(Kingdom kingdom)
    {
        if (string.IsNullOrEmpty(kingdom.CurrentResearchTech))
            return (null, 0, 0);

        var tech = await _context.TechnologyDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TechType == kingdom.CurrentResearchTech);
        if (tech == null) return (null, 0, 0);

        var research = await _context.Researches.AsNoTracking()
            .FirstOrDefaultAsync(r => r.KingdomId == kingdom.Id && r.TechType == kingdom.CurrentResearchTech);

        return (tech.DisplayName, research?.InvestedScience ?? 0, tech.CostScience);
    }

    /// <summary>Zdarzenia odnotowane od ostatniego przeliczenia (5:00) — pokazywane na Stolicy.</summary>
    private async Task<List<KingdomEventDto>> GetRecentEventsAsync(int kingdomId)
    {
        var todayReset = DateTime.Today.AddHours(5);
        var lastResetLocal = DateTime.Now >= todayReset ? todayReset : todayReset.AddDays(-1);
        var sinceUtc = lastResetLocal.ToUniversalTime();

        return await _context.KingdomEvents
            .Where(e => e.KingdomId == kingdomId && e.CreatedAt >= sinceUtc)
            .OrderByDescending(e => e.CreatedAt)
            .Take(30)
            .Select(e => new KingdomEventDto
            {
                Category = e.Category,
                Message = e.Message,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    // Rasy oryginalnego Red Dragon: 10 z reddragon.cz + Gnom i Br-Oug
    // z polskiego serwera reddragon.pl (definicje: RaceDefinitions, docs/MECHANIKA.md)
    public static readonly HashSet<string> AllRaces = new()
    {
        "Człowiek", "Elf", "Krasnolud", "Hobbit", "Nekromant",
        "Dżin", "Goblin", "Ent", "Olbrzym", "Gnom", "Br-Oug"
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
            Land = 1000,
            // Bufory startowe dobrane tak, by przetrwać pierwsze tury rozruchu
            // (rampa nowicjuszy + budowa/rekrutacja), nie wpadając od razu w spiralę bankructwa.
            Gold = 75000,
            Food = 15000,
            Stone = 5000,
            Budulec = 0,
            BudulecStored = 0,
            Weapons = 0,
            Mana = 0,
            Population = 1000,
            Popularity = 100,
            Wages = 50,
            Education = 0,
            TurnsAvailable = turnsPerDay,
            TurnsCapacity = turnsPerDay,
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
        // kupcy, alchemicy, płatnerze, druidzi, magowie, naukowcy.
        // Startowa, WYSZKOLONA kadra (NoviceCount=0): księstwo jest samowystarczalne
        // już w turze 0 (dodatni bilans złota, jedzenia i kamienia), a gracz rozwija je
        // dalej zatrudniając bezrobotnych. Bez tego cała ludność zaczynała jako nowicjusze
        // (10% produkcji) i ekonomia od razu wpadała w spiralę bankructwa.
        // Bilans przy płacy 50: złoto +2500/t (alchemicy 250×100 − pensje 450×50),
        // jedzenie +500/t (chłopi 150×10 − ludność 1000×1), kamień +250/t (kamieniarze 50×5).
        var startingWorkers = new Dictionary<string, int>
        {
            ["Bezrobotni"] = 550,
            ["Alchemicy"] = 250,
            ["Chłopi"] = 150,
            ["Kamieniarze"] = 50,
            ["Druidzi"] = 0,
            ["Murarze"] = 0,
            ["Płatnerze"] = 0,
            ["Kupcy"] = 0,
            ["Magowie"] = 0,
            ["Naukowcy"] = 0
        };

        foreach (var (profType, count) in startingWorkers)
        {
            _context.Professions.Add(new Profession
            {
                KingdomId = kingdom.Id,
                ProfessionType = profType,
                WorkerCount = count,
                NoviceCount = 0,   // kadra startowa jest już wyszkolona (pełna produktywność)
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
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

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

            // Limit zawodu (baza + cechy/uniwersytety) — by zatrudnić więcej, trzeba rozbudować budynki.
            int capacity = ProfessionCapacity(kingdom, dto.ProfessionType);
            if (targetProfession.WorkerCount + dto.WorkerCount > capacity)
                return ServiceResult.Fail(
                    $"Limit zawodu {targetProfession.ProfessionType}: {capacity}. " +
                    $"Obecnie {targetProfession.WorkerCount}. Rozbuduj odpowiednie budynki (cechy / uniwersytety), aby zwiększyć limit.");

            unemployed.WorkerCount -= dto.WorkerCount;
            targetProfession.WorkerCount += dto.WorkerCount;
            // Świeżo zatrudnieni są nowicjuszami (produkują 10%, płacą połowę pensji)
            // do czasu wyszkolenia w Szkołach — docs/MECHANIKA.md §9, ResourceService.
            targetProfession.NoviceCount += dto.WorkerCount;
        }
        else
        {
            // Zwalnianie pracowników (ujemna wartość)
            int toFree = Math.Abs(dto.WorkerCount);
            if (targetProfession.WorkerCount < toFree)
                return ServiceResult.Fail($"Za mało pracowników w tej profesji. Aktualnie: {targetProfession.WorkerCount}");

            // Zwalniamy proporcjonalnie nowicjuszy i wyszkolonych, aby % nowicjuszy nie skakał.
            int freedNovices = targetProfession.WorkerCount > 0
                ? (int)((long)toFree * targetProfession.NoviceCount / targetProfession.WorkerCount)
                : 0;
            targetProfession.WorkerCount -= toFree;
            targetProfession.NoviceCount = Math.Max(0, targetProfession.NoviceCount - freedNovices);
            if (targetProfession.NoviceCount > targetProfession.WorkerCount)
                targetProfession.NoviceCount = targetProfession.WorkerCount;
            unemployed.WorkerCount += toFree;
        }

        targetProfession.NovicePercent = targetProfession.WorkerCount > 0
            ? (decimal)targetProfession.NoviceCount / targetProfession.WorkerCount * 100m
            : 0m;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Pracownicy zostali przydzieleni.");
    }

    public async Task<ServiceResult> BuyLandAsync(int userId, int amount)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        if (amount <= 0)
            return ServiceResult.Fail("Nieprawidłowa ilość.");

        // Oryginalny wzór: cena = ((z+x)^3,5 − z^3,5) / 600 000
        long cost = CalculateLandCost(kingdom.Land, amount);
        // Rabat badań: Rekultywacja/Osadnictwo (LandCostReduction)
        decimal landDiscount = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "LandCostReduction");
        cost = (long)(cost * (1m - landDiscount));
        // Protektorat początkowy (Dracopedia §1): ziemia −60%.
        if (kingdom.IsProtected) cost = (long)(cost * 0.4m);
        if (kingdom.Gold < cost)
            return ServiceResult.Fail($"Za mało złota. Potrzeba: {cost}, posiadasz: {kingdom.Gold}");

        kingdom.Gold -= cost;
        kingdom.Land += amount;

        // „Parasol" nowicjusza spada po przekroczeniu progu obszaru (Kingdom.NoviceLandCap).
        if (kingdom.IsProtected && kingdom.Land >= Kingdom.NoviceLandCap)
        {
            kingdom.IsProtected = false;
            kingdom.ProtectionDaysLeft = 0;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zakupiono {amount} akrów za {cost} złota.");
    }

    public async Task<ServiceResult> FreezeAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (!kingdom.IsFrozen) return ServiceResult.Fail("Księstwo nie jest zamrożone.");

        kingdom.IsFrozen = false;
        kingdom.FrozenAt = null;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Księstwo odmrożone — możesz znów działać.");
    }

    public async Task<ServiceResult> SetWagesAsync(int userId, int wages)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");

        // Płaca 0–50 złota na pracownika/turę (manual: max 50 = 100 popularności).
        wages = Math.Clamp(wages, 0, 50);
        kingdom.Wages = wages;
        await _context.SaveChangesAsync();

        int target = Math.Min(100, wages * 2);
        return ServiceResult.Ok($"Ustawiono pensję na {wages} złota/pracownika. Docelowa popularność: {target}%.");
    }

    public async Task<ServiceResult> DropProtectionAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (!kingdom.IsProtected) return ServiceResult.Fail("Twoje księstwo nie jest objęte ochroną.");

        kingdom.IsProtected = false;
        kingdom.ProtectionDaysLeft = 0;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Ochrona zdjęta — Twoje księstwo może teraz atakować i być atakowane.");
    }

    public async Task<ServiceResult> SetMetamagicAsync(int userId, string mode)
    {
        if (mode is not ("None" or "Strengthened" or "Accelerated"))
            return ServiceResult.Fail("Nieznany tryb metamagii.");

        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Dżin") return ServiceResult.Fail("Metamagia dostępna tylko dla Dżina.");

        kingdom.MetamagicMode = mode;
        await _context.SaveChangesAsync();
        string label = mode switch
        {
            "Strengthened" => "wzmocniona (+10% siły, +25% ceny)",
            "Accelerated" => "przyspieszona (−25% siły, −10% ceny)",
            _ => "wyłączona"
        };
        return ServiceResult.Ok($"Metamagia: {label}.");
    }

    public async Task<ServiceResult> ChargeTotemAsync(int userId, string totem)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Olbrzym") return ServiceResult.Fail("Szamanizm dostępny tylko dla Olbrzyma.");

        int level = totem switch
        {
            "Plunder" => kingdom.TotemPlunder,
            "DragonSlay" => kingdom.TotemDragonSlay,
            "Destruction" => kingdom.TotemDestruction,
            _ => -1
        };
        if (level < 0) return ServiceResult.Fail("Nieznany totem.");
        if (level >= 10) return ServiceResult.Fail("Totem jest maksymalnie naładowany.");

        long cost = (long)kingdom.Land * 20;
        if (kingdom.Mana < cost)
            return ServiceResult.Fail($"Za mało many (potrzeba {cost}, masz {kingdom.Mana}).");

        kingdom.Mana -= cost;
        switch (totem)
        {
            case "Plunder": kingdom.TotemPlunder++; break;
            case "DragonSlay": kingdom.TotemDragonSlay++; break;
            case "Destruction": kingdom.TotemDestruction++; break;
        }
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Naładowano totem do poziomu {level + 1} (koszt {cost} many).");
    }

    public async Task<ServiceResult> RearmAsync(int userId, string tier, string stat)
    {
        if (tier is not ("E1" or "E2") || stat is not ("Attack" or "Defense"))
            return ServiceResult.Fail("Nieznany cel dozbrojenia.");

        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Krasnolud")
            return ServiceResult.Fail("Dozbrojenie dostępne tylko dla Krasnoluda.");

        int points = kingdom.RearmE1Attack + kingdom.RearmE1Defense
                     + kingdom.RearmE2Attack + kingdom.RearmE2Defense;
        if (points >= 2)
            return ServiceResult.Fail("Oba punkty dozbrojenia są już wykorzystane (reset po przeliczeniu).");

        // Blog 31. wieku: 1. punkt = ziemia×50 broni, 2. punkt = ziemia×100
        long cost = (long)kingdom.Land * (points == 0 ? 50 : 100);
        if (kingdom.Weapons < cost)
            return ServiceResult.Fail($"Za mało broni (potrzeba {cost}, masz {kingdom.Weapons}).");

        kingdom.Weapons -= cost;
        switch ((tier, stat))
        {
            case ("E1", "Attack"): kingdom.RearmE1Attack++; break;
            case ("E1", "Defense"): kingdom.RearmE1Defense++; break;
            case ("E2", "Attack"): kingdom.RearmE2Attack++; break;
            default: kingdom.RearmE2Defense++; break;
        }
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Dozbrojono {tier}: +1 {(stat == "Attack" ? "ataku" : "obrony")} (koszt {cost} broni, do przeliczenia).");
    }

    public async Task<ServiceResult> SetArcherCommandoAsync(int userId, int? targetKingdomId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Elf")
            return ServiceResult.Fail("Komando łuczników dostępne tylko dla Elfa.");

        if (targetKingdomId == null)
        {
            if (kingdom.ArcherCommandoTargetId == null)
                return ServiceResult.Fail("Komando nie jest wysłane.");
            kingdom.ArcherCommandoTargetId = null;
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Komando łuczników wróciło do domu.");
        }

        if (targetKingdomId == kingdom.Id)
            return ServiceResult.Fail("Nie można wysłać komanda do własnego księstwa.");
        var target = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == targetKingdomId);
        if (target == null) return ServiceResult.Fail("Nie znaleziono księstwa docelowego.");
        if (target.Race == "Elf")
            return ServiceResult.Fail("Elfy nie mogą przyjmować komanda łuczników.");

        var partners = await PactService.GetActivePactPartnersAsync(_context, kingdom.Id, "Wojskowy");
        if (partners.All(p => p.Partner.Id != targetKingdomId))
            return ServiceResult.Fail("Komando można wysłać tylko do księstwa z aktywnym paktem wojskowym.");
        if (await _context.Kingdoms.AnyAsync(k => k.Id != kingdom.Id && k.ArcherCommandoTargetId == targetKingdomId))
            return ServiceResult.Fail("To księstwo przyjęło już komando innego Elfa.");

        kingdom.ArcherCommandoTargetId = targetKingdomId;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Komando łuczników wspiera {target.Name}: +20% jego obrony, −20% własnej (do przeliczenia).");
    }

    public async Task<ServiceResult> SetHodokvasAsync(int userId, bool active)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Hobbit")
            return ServiceResult.Fail("Hodokvas dostępny tylko dla Hobbita.");

        if (active)
        {
            if (kingdom.HodokvasActive) return ServiceResult.Fail("Hodokvas już trwa.");
            if (kingdom.Popularity < 80)
                return ServiceResult.Fail("Hodokvas wymaga popularności co najmniej 80.");
            kingdom.HodokvasActive = true;
            kingdom.HodokvasTurnsPlayed = 0;
            kingdom.Popularity += 20; // może przekroczyć 100 — podnosi zaludnienie na akr
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Hodokvas rozpoczęty: +20 popularności, jedzenie 5/osobę, szkolenie −40%, przyrost +50%.");
        }

        if (!kingdom.HodokvasActive) return ServiceResult.Fail("Hodokvas nie trwa.");
        if (kingdom.HodokvasTurnsPlayed < 4)
            return ServiceResult.Fail($"Hodokvas można zakończyć najwcześniej po 4 turach (odegrano {kingdom.HodokvasTurnsPlayed}).");
        kingdom.HodokvasActive = false;
        // Blog 31. wieku: po zakończeniu popularność wraca do 100 (jeśli była ≥100)
        if (kingdom.Popularity >= 100) kingdom.Popularity = 100;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Hodokvas zakończony — spożycie jedzenia i szkolenie wracają do normy.");
    }

    public async Task<ServiceResult> SetAppliedScienceAsync(int userId, string school)
    {
        if (school is not ("None" or "Thief" or "Magic" or "Military"))
            return ServiceResult.Fail("Nieznana szkoła.");

        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.Race != "Człowiek") return ServiceResult.Fail("Nauka stosowana dostępna tylko dla Człowieka.");

        kingdom.AppliedScienceSchool = school;
        await _context.SaveChangesAsync();
        string label = school switch
        {
            "Thief" => "złodziejska (+10% siły złodziei)",
            "Magic" => "magiczna (+10% siły zaklęć)",
            "Military" => "wojskowa (+10% ataku)",
            _ => "wyłączona"
        };
        return ServiceResult.Ok($"Nauka stosowana: szkoła {label}.");
    }

    public async Task<ServiceResult> ChangeRaceAsync(int userId, string race)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Include(k => k.MilitaryUnits)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return ServiceResult.Fail("Nie znaleziono księstwa.");

        bool hasPalace = kingdom.Buildings.Any(b =>
            b.BuildingType == "PalacZmian" && b.Quantity > 0 && !b.IsUnderConstruction);
        if (!hasPalace) return ServiceResult.Fail("Wymagany Pałac Zmian.");
        if (race == kingdom.Race) return ServiceResult.Fail("To już Twoja rasa.");

        var raceDef = await _context.RaceDefinitions.FirstOrDefaultAsync(r => r.Name == race);
        if (raceDef == null) return ServiceResult.Fail("Nieznana rasa.");
        if (kingdom.TurnsAvailable <= 0) return ServiceResult.Fail("Brak dostępnych tur.");

        kingdom.TurnsAvailable--;
        kingdom.Race = race;
        kingdom.IsMagicRace = raceDef.MagicBooks > 0;
        kingdom.TurnsPerDay = raceDef.TurnsPerDay;
        kingdom.MaxTurns = raceDef.TurnsPerDay * 3 + 4;

        // Reset mechanik rasowych poprzedniej rasy
        kingdom.MetamagicMode = "None";
        kingdom.AppliedScienceSchool = "None";
        kingdom.EntWrathActive = false;
        kingdom.Bodies = 0;
        kingdom.TotemPlunder = kingdom.TotemDragonSlay = kingdom.TotemDestruction = 0;

        // Jednostki są rasowe — armia poprzedniej rasy zostaje rozwiązana
        _context.MilitaryUnits.RemoveRange(kingdom.MilitaryUnits);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zmieniono rasę na {race}. Armia poprzedniej rasy została rozwiązana.");
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
                CoalitionId = k.CoalitionId,
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

    /// <summary>
    /// Maksymalna liczba pracowników, jaką można przydzielić do danej profesji.
    /// Baza startowa + przyrost za odpowiednie budynki (cechy / uniwersytety) —
    /// aby zatrudnić więcej, trzeba je rozbudować. „Bezrobotni" to pula bez limitu (0 = n/d).
    /// </summary>
    public static int ProfessionCapacity(Kingdom kingdom, string professionType)
    {
        if (professionType == "Bezrobotni") return 0;

        int Count(string bt) => kingdom.Buildings
            .Where(b => b.BuildingType == bt && !b.IsUnderConstruction)
            .Sum(b => b.Quantity);

        const int Base = 1000;        // każdy zawód można obsadzić startowo do 1000 osób
        const int PerBuilding = 2000; // każdy odpowiedni budynek dokłada miejsca

        int buildings = professionType switch
        {
            "Alchemicy" or "Chłopi" => Count("CechSlonca"),
            "Druidzi" or "Kamieniarze" => Count("CechZiemi"),
            "Murarze" or "Płatnerze" => Count("CechGwiazd"),
            "Naukowcy" => Count("Uniwersytety"),
            _ => 0 // Kupcy, Magowie — tylko baza
        };

        return Base + buildings * PerBuilding;
    }

    private static KingdomDto MapToDto(Kingdom kingdom, int pendingGeneralCount = 0,
        List<KingdomEventDto>? recentEvents = null,
        (string? Name, long Progress, long Cost) research = default)
    {
        // Zabudowa ziemi: zajęta ziemia = Σ ilość × koszt ziemi z definicji budynku.
        int usedLand = kingdom.Buildings.Sum(b => b.Quantity * (b.Definition?.CostLand ?? 0));
        int freeLand = Math.Max(0, kingdom.Land - usedLand);
        decimal builtPercent = kingdom.Land > 0 ? Math.Round((decimal)usedLand / kingdom.Land * 100m, 1) : 0m;
        int housingLand = kingdom.Buildings
            .Where(b => b.BuildingType == "Domy")
            .Sum(b => b.Quantity * (b.Definition?.CostLand ?? 0));
        decimal housingPercent = kingdom.Land > 0 ? Math.Round((decimal)housingLand / kingdom.Land * 100m, 1) : 0m;

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
            MetamagicMode = kingdom.MetamagicMode,
            EntWrathActive = kingdom.EntWrathActive,
            TotemPlunder = kingdom.TotemPlunder,
            TotemDragonSlay = kingdom.TotemDragonSlay,
            TotemDestruction = kingdom.TotemDestruction,
            AppliedScienceSchool = kingdom.AppliedScienceSchool,
            RearmE1Attack = kingdom.RearmE1Attack,
            RearmE1Defense = kingdom.RearmE1Defense,
            RearmE2Attack = kingdom.RearmE2Attack,
            RearmE2Defense = kingdom.RearmE2Defense,
            RearmNextCost = (kingdom.RearmE1Attack + kingdom.RearmE1Defense
                             + kingdom.RearmE2Attack + kingdom.RearmE2Defense) switch
            {
                0 => (long)kingdom.Land * 50,
                1 => (long)kingdom.Land * 100,
                _ => 0
            },
            ArcherCommandoTargetId = kingdom.ArcherCommandoTargetId,
            HodokvasActive = kingdom.HodokvasActive,
            HodokvasTurnsPlayed = kingdom.HodokvasTurnsPlayed,
            Population = kingdom.Population,
            Popularity = kingdom.Popularity,
            Wages = kingdom.Wages,
            Education = kingdom.Education,
            TurnsAvailable = kingdom.TurnsAvailable,
            TurnsCapacity = kingdom.TurnsCapacity,
            TurnsPerDay = kingdom.TurnsPerDay,
            MaxTurns = kingdom.MaxTurns,
            TurnNumber = kingdom.TurnNumber,
            Age = kingdom.Age,
            UsedLand = usedLand,
            FreeLand = freeLand,
            BuiltPercent = builtPercent,
            HousingLand = housingLand,
            HousingPercent = housingPercent,
            CurrentSpecialBuilding = string.IsNullOrEmpty(kingdom.CurrentSpecialBuilding)
                ? null
                : (kingdom.Buildings.FirstOrDefault(b => b.BuildingType == kingdom.CurrentSpecialBuilding)?.Definition?.DisplayName
                   ?? kingdom.CurrentSpecialBuilding),
            SpecialBuildingProgress = kingdom.SpecialBuildingProgress,
            SpecialBuildingCost = kingdom.SpecialBuildingCost,
            CurrentResearch = research.Name,
            CurrentResearchTech = kingdom.CurrentResearchTech,
            ResearchProgress = research.Progress,
            ResearchCost = research.Cost,
            SciencePoints = kingdom.SciencePoints,
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
                MaxCapacity = ProfessionCapacity(kingdom, p.ProfessionType),
                ProductionPerTurn = p.ProductionPerTurn,
                NovicePercent = p.NovicePercent
            }).ToList(),
            PendingGeneralCount = pendingGeneralCount,
            ActiveSpells = kingdom.ActiveSpells.Select(s => new ActiveSpellDto
            {
                SpellType = s.SpellType,
                DisplayName = s.Spell?.DisplayName ?? s.SpellType,
                Category = s.Spell?.Category ?? "",
                Power = s.Power,
                IsPositive = s.Spell != null &&
                    (s.Spell.Category == "Biała" || s.Spell.Category == "Tarcze")
            }).ToList(),
            RecentEvents = recentEvents ?? new List<KingdomEventDto>()
        };
    }
}
