using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IGeneralService
{
    Task<List<GeneralDto>> GetGeneralsAsync(int userId);
    Task<ServiceResult> AcceptGeneralAsync(int userId, int generalId);
    Task<ServiceResult> RerollSecondaryTraitAsync(int userId, int generalId);
    Task<ServiceResult> DismissGeneralAsync(int userId, int generalId);
    Task ProcessGeneralArrivalsAsync();
    Task<bool> TryGeneralArrivalAsync(Kingdom kingdom);

    /// <summary>
    /// Labirynt — próba znalezienia generała (akcja „szukaj generała", docs/MECHANIKA.md §13).
    /// Dodaje oczekującego generała do wspólnego DbContext (bez zapisu) i zwraca, czy się udało.
    /// Szansa rośnie z poziomem wysłanego generała i siłą Szczęścia. Respektuje limit i poczekalnię.
    /// </summary>
    Task<bool> TryLabyrinthFindGeneralAsync(Kingdom kingdom, int searchLevel, int fortunePct);

    /// <summary>
    /// Labirynt — Ołtarz: zmienia cechę drugorzędną wybranego generała na nową losową,
    /// kosztem połowy doświadczenia (docs/MECHANIKA.md §13). Nie zapisuje zmian.
    /// </summary>
    string ChangeSecondaryFromAltar(General general);
}

/// <summary>
/// Generałowie wg oryginału (docs/MECHANIKA.md §11): limit 6 (8 z Pałacem;
/// Człowiek zawsze 8), przychodzą losowo — im bliżej limitu, tym rzadziej;
/// Akademia dowodzenia podwaja szansę.
/// </summary>
public class GeneralService : IGeneralService
{
    private readonly ApplicationDbContext _context;

    private static readonly string[] Names =
    {
        "Aldaron", "Borivoj", "Cedrik", "Dagomir", "Eldur", "Falkrim", "Gormund",
        "Haldor", "Ivellios", "Jarogniew", "Kazimir", "Lothar", "Mirgost", "Norbald",
        "Oswin", "Przemir", "Radowit", "Svarog", "Theoden", "Uldred", "Velimir",
        "Wszebor", "Yorick", "Zbigniew", "Almaria", "Brenna", "Cyryna", "Dalia",
        "Elwira", "Freya", "Gwenna", "Halina", "Isolda", "Jaga", "Kasylda", "Lutomira"
    };

    // Cechy główne (zdobywanie doświadczenia)
    private static readonly string[] PrimaryTraits =
        { "Wodz", "Obronca", "Mag", "Zlodziej", "Kupiec", "Profesor" };

    // Dozwolone kombinacje cech drugorzędnych wg manuala
    private static readonly Dictionary<string, string[]> AllowedSecondary = new()
    {
        ["Wodz"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "Sabotaz", "Krwiozerczonsc", "Rabunek", "MaskowanieISzpiegostwo",
            "CzarnaMagia", "MagiaCzasu"
        },
        ["Obronca"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia"
        },
        ["Mag"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia", "MaskowanieISzpiegostwo"
        },
        ["Zlodziej"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia", "MaskowanieISzpiegostwo"
        },
        ["Kupiec"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia", "Odkrywca"
        },
        ["Profesor"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia", "Odkrywca"
        }
    };

    public GeneralService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GeneralDto>> GetGeneralsAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null) return new List<GeneralDto>();

        var generals = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id)
            .OrderByDescending(g => g.Experience)
            .ToListAsync();

        return generals.Select(g => new GeneralDto
        {
            Id = g.Id,
            Name = g.Name,
            PrimaryTrait = g.PrimaryTrait,
            SecondaryTrait = g.SecondaryTrait,
            Experience = g.Experience,
            Level = g.Level,
            ExperienceToNextLevel = g.ExperienceToNextLevel,
            IsPending = g.IsPending,
            SecondaryRerollsLeft = Math.Max(0, 2 - g.SecondaryRerollsUsed),
            Status = g.IsPending ? "Oczekuje na decyzję"
                : g.IsImprisoned ? "Więziony"
                : g.IsOutside ? "Poza księstwem"
                : g.WoundedUntil.HasValue && g.WoundedUntil > DateTime.UtcNow ? "Ranny"
                : "W domu"
        }).ToList();
    }

    public async Task<ServiceResult> AcceptGeneralAsync(int userId, int generalId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == generalId && g.KingdomId == kingdom.Id && g.IsPending);
        if (general == null)
            return ServiceResult.Fail("Nie znaleziono oczekującego generała.");

        general.IsPending = false;
        general.ArrivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Generał {general.Name} dołączył do twojego księstwa.");
    }

    /// <summary>
    /// Przelosowanie cechy drugorzędnej oczekującego generała (przy zatrudnianiu, maks. 2 razy).
    /// </summary>
    public async Task<ServiceResult> RerollSecondaryTraitAsync(int userId, int generalId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == generalId && g.KingdomId == kingdom.Id && g.IsPending);
        if (general == null)
            return ServiceResult.Fail("Nie znaleziono oczekującego generała.");
        if (general.SecondaryRerollsUsed >= 2)
            return ServiceResult.Fail("Wykorzystałeś już obie próby zmiany cechy drugorzędnej.");

        // Losujemy nową cechę, najlepiej inną niż obecna
        var options = AllowedSecondary[general.PrimaryTrait]
            .Where(s => s != general.SecondaryTrait).ToArray();
        if (options.Length == 0) options = AllowedSecondary[general.PrimaryTrait];

        general.SecondaryTrait = options[Random.Shared.Next(options.Length)];
        general.SecondaryRerollsUsed++;
        await _context.SaveChangesAsync();

        int left = Math.Max(0, 2 - general.SecondaryRerollsUsed);
        return ServiceResult.Ok($"Nowa cecha drugorzędna: {general.SecondaryTrait}. Pozostałe próby: {left}.");
    }

    public async Task<ServiceResult> DismissGeneralAsync(int userId, int generalId)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == generalId && g.KingdomId == kingdom.Id);
        if (general == null)
            return ServiceResult.Fail("Nie znaleziono generała.");
        if (general.IsOutside)
            return ServiceResult.Fail("Generał prowadzi atak — nie można go teraz zwolnić.");

        _context.Generals.Remove(general);
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Generał {general.Name} został zwolniony ze służby.");
    }

    /// <summary>Wywoływane przy dziennym przeliczeniu: próba przyjścia generała dla każdego księstwa.</summary>
    public async Task ProcessGeneralArrivalsAsync()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Where(k => k.Era.IsActive)
            .ToListAsync();

        // Aktywni (bez oczekujących) na potrzeby limitu/szansy oraz zbiór księstw z oczekującym kandydatem
        var activeCounts = await _context.Generals
            .Where(g => !g.IsPending)
            .GroupBy(g => g.KingdomId)
            .Select(g => new { KingdomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.KingdomId, x => x.Count);

        var pendingKingdoms = (await _context.Generals
            .Where(g => g.IsPending)
            .Select(g => g.KingdomId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        foreach (var kingdom in kingdoms)
            TryArrival(kingdom, activeCounts.GetValueOrDefault(kingdom.Id, 0),
                pendingKingdoms.Contains(kingdom.Id),
                await GetGeneralsLimitAsync(kingdom));

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Próba przyjścia generała dla pojedynczego księstwa — wywoływane przy każdej użytej turze
    /// (manual: „If you have none, a new General surely comes in the next turn").
    /// Nowy generał trafia do poczekalni (IsPending) i czeka na decyzję gracza.
    /// Zakłada, że kingdom.Buildings są już załadowane. Zapisuje zmiany, jeśli generał przyszedł.
    /// </summary>
    public async Task<bool> TryGeneralArrivalAsync(Kingdom kingdom)
    {
        int activeCount = await _context.Generals.CountAsync(g => g.KingdomId == kingdom.Id && !g.IsPending);
        bool hasPending = await _context.Generals.AnyAsync(g => g.KingdomId == kingdom.Id && g.IsPending);
        int limit = await GetGeneralsLimitAsync(kingdom);

        // Cecha ostatnio przybyłego generała — by nie generować serii tej samej cechy
        // (np. „same kupcy”). Kolejny kandydat dostanie inną cechę główną, jeśli to możliwe.
        string? lastPrimary = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id)
            .OrderByDescending(g => g.ArrivedAt)
            .Select(g => g.PrimaryTrait)
            .FirstOrDefaultAsync();

        bool arrived = TryArrival(kingdom, activeCount, hasPending, limit, lastPrimary);
        if (arrived) await _context.SaveChangesAsync();
        return arrived;
    }

    public async Task<bool> TryLabyrinthFindGeneralAsync(Kingdom kingdom, int searchLevel, int fortunePct)
    {
        int limit = await GetGeneralsLimitAsync(kingdom);
        int activeCount = await _context.Generals
            .CountAsync(g => g.KingdomId == kingdom.Id && !g.IsPending);
        bool hasPending = await _context.Generals
            .AnyAsync(g => g.KingdomId == kingdom.Id && g.IsPending);

        if (hasPending || activeCount >= limit) return false;

        // Szansa bazowa 45% + poziom wysłanego generała i fart (Szczęście) zwiększają trafienie.
        double chance = 0.45 + Math.Min(0.35, searchLevel * 0.015) + fortunePct / 200.0;
        if (Random.Shared.NextDouble() >= Math.Min(0.95, chance)) return false;

        string primary = PrimaryTraits[Random.Shared.Next(PrimaryTraits.Length)];
        var secondaries = AllowedSecondary[primary];
        string secondary = secondaries[Random.Shared.Next(secondaries.Length)];

        _context.Generals.Add(new General
        {
            KingdomId = kingdom.Id,
            Name = Names[Random.Shared.Next(Names.Length)],
            PrimaryTrait = primary,
            SecondaryTrait = secondary,
            Experience = 0,
            IsPending = true,
            ArrivedAt = DateTime.UtcNow
        });
        return true;
    }

    public string ChangeSecondaryFromAltar(General general)
    {
        var options = AllowedSecondary[general.PrimaryTrait]
            .Where(s => s != general.SecondaryTrait).ToArray();
        if (options.Length == 0) options = AllowedSecondary[general.PrimaryTrait];

        general.SecondaryTrait = options[Random.Shared.Next(options.Length)];
        general.Experience /= 2; // Ołtarz pobiera połowę doświadczenia za przemianę
        return general.SecondaryTrait;
    }

    /// <summary>Limit aktywnych generałów: bazowy z rasy, +Pałac do 8.</summary>
    private async Task<int> GetGeneralsLimitAsync(Kingdom kingdom)
    {
        var race = await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == kingdom.Race);
        int limit = race?.GeneralsLimit ?? 6;
        // Pałac podnosi limit do 8 (rasy z limitem 8 mają go zawsze)
        bool hasPalace = kingdom.Buildings.Any(b =>
            b.BuildingType == "RezydencjaGenerala" && b.Quantity > 0 && !b.IsUnderConstruction);
        if (hasPalace) limit = Math.Max(limit, 8);
        return limit;
    }

    /// <summary>
    /// Wspólna logika losowania przyjścia. Przy 0 aktywnych generałów przyjście jest gwarantowane;
    /// dalej szansa maleje z liczbą generałów, a Akademia dowodzenia ją podwaja.
    /// Gdy w poczekalni już ktoś czeka — nie przychodzi kolejny. Nie zapisuje zmian.
    /// </summary>
    private bool TryArrival(Kingdom kingdom, int activeCount, bool hasPending, int limit, string? excludePrimary = null)
    {
        // Jeden kandydat na raz — dopóki gracz nie zdecyduje, nowi nie przychodzą
        if (hasPending) return false;

        if (activeCount >= limit) return false;

        double chance;
        if (activeCount == 0)
        {
            chance = 1.0; // wg manuala: gdy nie masz generała, kolejny przychodzi na pewno
        }
        else
        {
            // szansa maleje z liczbą generałów; Akademia dowodzenia podwaja.
            // Wysoka baza jest bezpieczna: naraz czeka tylko jeden kandydat (musisz go przyjąć/odrzucić),
            // więc per-tura kontroluje tylko odstęp między kolejnymi kandydatami.
            chance = 0.35 * (1.0 - (double)activeCount / limit);
            if (kingdom.Buildings.Any(b =>
                    b.BuildingType == "AkademiaWojskowa" && b.Quantity > 0 && !b.IsUnderConstruction))
                chance *= 2;
        }

        if (Random.Shared.NextDouble() >= chance) return false;

        // Losuj cechę główną, unikając powtórzenia po ostatnio przybyłym generale.
        var primaryPool = excludePrimary != null
            ? PrimaryTraits.Where(t => t != excludePrimary).ToArray()
            : PrimaryTraits;
        if (primaryPool.Length == 0) primaryPool = PrimaryTraits;
        string primary = primaryPool[Random.Shared.Next(primaryPool.Length)];
        var secondaries = AllowedSecondary[primary];
        string secondary = secondaries[Random.Shared.Next(secondaries.Length)];

        _context.Generals.Add(new General
        {
            KingdomId = kingdom.Id,
            Name = Names[Random.Shared.Next(Names.Length)],
            PrimaryTrait = primary,
            SecondaryTrait = secondary,
            Experience = 0,
            IsPending = true, // czeka na decyzję gracza (przyjąć / odrzucić)
            ArrivedAt = DateTime.UtcNow
        });
        return true;
    }
}
