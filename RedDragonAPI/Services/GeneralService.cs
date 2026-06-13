using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IGeneralService
{
    Task<List<GeneralDto>> GetGeneralsAsync(int userId);
    Task<ServiceResult> DismissGeneralAsync(int userId, int generalId);
    Task ProcessGeneralArrivalsAsync();
}

/// <summary>
/// Generałowie wg oryginału (docs/MECHANIKA.md §11): limit 6 (8 z Pałacem;
/// Człowiek i Wampir zawsze 8), przychodzą losowo — im bliżej limitu, tym rzadziej;
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
            "Uzdrawianie", "BialaMagia"
        },
        ["Profesor"] = new[]
        {
            "PorwanieGenerala", "ZabojstwoGenerala", "ZranienieGenerala", "Smokobojstwo",
            "Uzdrawianie", "BialaMagia"
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
            Status = g.IsImprisoned ? "Więziony"
                : g.IsOutside ? "Poza księstwem"
                : g.WoundedUntil.HasValue && g.WoundedUntil > DateTime.UtcNow ? "Ranny"
                : "W domu"
        }).ToList();
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

    /// <summary>Wywoływane przy przeliczeniu: przychodzenie nowych generałów.</summary>
    public async Task ProcessGeneralArrivalsAsync()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Buildings)
            .Where(k => k.Era.IsActive)
            .ToListAsync();

        var counts = await _context.Generals
            .GroupBy(g => g.KingdomId)
            .Select(g => new { KingdomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.KingdomId, x => x.Count);

        foreach (var kingdom in kingdoms)
        {
            var race = await _context.RaceDefinitions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == kingdom.Race);

            int limit = race?.GeneralsLimit ?? 6;
            // Pałac podnosi limit do 8 (rasy z limitem 8 mają go zawsze)
            bool hasPalace = kingdom.Buildings.Any(b =>
                b.BuildingType == "RezydencjaGenerala" && b.Quantity > 0 && !b.IsUnderConstruction);
            if (hasPalace) limit = Math.Max(limit, 8);

            int current = counts.GetValueOrDefault(kingdom.Id, 0);
            if (current >= limit) continue;
            if (!hasPalace && current >= 6) continue;

            // szansa maleje z liczbą generałów; Akademia dowodzenia podwaja
            double chance = 0.25 * (1.0 - (double)current / limit);
            if (kingdom.Buildings.Any(b =>
                    b.BuildingType == "AkademiaWojskowa" && b.Quantity > 0 && !b.IsUnderConstruction))
                chance *= 2;

            if (Random.Shared.NextDouble() >= chance) continue;

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
                ArrivedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
