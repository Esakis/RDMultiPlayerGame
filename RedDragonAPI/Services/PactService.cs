using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IPactService
{
    Task<PactStatusDto> GetPactStatusAsync(int userId);
    Task<ServiceResult> SetPactAsync(int userId, SetPactDto dto);
}

/// <summary>
/// Pakty wewnątrz koalicji wg modelu gry (docs/zrodla/urza-pakt.txt):
/// istnieją 4 typy paktów (Handlowy, Magiczny, Wojskowy, Zlodziejski). Z każdym
/// współczłonkiem koalicji można zawrzeć DOWOLNĄ KOMBINACJĘ tych paktów, ale tylko
/// jeden pakt danego typu — więc maksymalnie 4 różne pakty z jednym księstwem.
/// Każdy pakt to osobny rekord (wspólny dla obu stron, natychmiastowy/jednostronny).
/// Pakt handlowy dolicza obszar sojusznika do efektywności kupców; pakty obronne
/// (Magiczny/Wojskowy/Zlodziejski) wspomagają obronę. Limit łącznej liczby paktów:
/// baza 5 + Ambasada (+1). Skuteczność obronna wg liczby paktów danego typu: 50%/45%/40%.
/// </summary>
public class PactService : IPactService
{
    public const int BasePactLimit = 5; // +1 z Ambasadą
    public const string TradePactType = "Handlowy";

    /// <summary>Wszystkie dozwolone typy paktów.</summary>
    public static readonly string[] AllPactTypes =
        { "Handlowy", "Magiczny", "Wojskowy", "Zlodziejski" };

    /// <summary>Typy paktów obronnych (wspomagają obronę).</summary>
    public static readonly string[] DefensePactTypes =
        { "Magiczny", "Wojskowy", "Zlodziejski" };

    /// <summary>Skuteczność paktu obronnego wg liczby paktów danego typu: 50%/45%/40%.</summary>
    public static decimal PactEfficiency(int pactCountOfType) => pactCountOfType switch
    {
        <= 1 => 0.50m,
        2 => 0.45m,
        _ => 0.40m
    };

    private readonly ApplicationDbContext _context;

    public PactService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Kingdom?> GetKingdomAsync(int userId) =>
        await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

    private static bool HasAmbasada(Kingdom k) =>
        k.Buildings.Any(b => b.BuildingType == "Ambasada" && b.Quantity > 0 && !b.IsUnderConstruction);

    private static int LimitFor(Kingdom k) => BasePactLimit + (HasAmbasada(k) ? 1 : 0);

    public async Task<PactStatusDto> GetPactStatusAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null) return new PactStatusDto();

        var dto = new PactStatusDto
        {
            HasAmbasada = HasAmbasada(kingdom),
            Limit = LimitFor(kingdom),
            InCoalition = kingdom.CoalitionId != null
        };
        if (kingdom.CoalitionId == null) return dto;

        var pacts = await _context.Pacts
            .Where(p => p.Status == "Active"
                        && (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id))
            .ToListAsync();

        // Aktywne typy paktów per partner (może być kilka typów z jednym księstwem).
        var typesByPartner = pacts
            .GroupBy(p => p.ProposerKingdomId == kingdom.Id ? p.TargetKingdomId : p.ProposerKingdomId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PactType).Distinct().ToList());
        dto.UsedSlots = pacts.Count;

        var members = await _context.Kingdoms
            .Where(k => k.CoalitionId == kingdom.CoalitionId && k.Id != kingdom.Id)
            .Select(k => new { k.Id, k.Name, k.Race, k.Land })
            .ToListAsync();

        dto.Members = members
            .Select(m => new PactMemberDto
            {
                KingdomId = m.Id,
                Name = m.Name,
                Race = m.Race,
                Land = m.Land,
                ActivePacts = typesByPartner.TryGetValue(m.Id, out var t) ? t : new List<string>()
            })
            .OrderBy(m => m.Name)
            .ToList();

        return dto;
    }

    public async Task<ServiceResult> SetPactAsync(int userId, SetPactDto dto)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null)
            return ServiceResult.Fail("Pakty można zawierać tylko w obrębie koalicji.");

        var target = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);
        if (target == null)
            return ServiceResult.Fail("Nie znaleziono księstwa partnera.");
        if (target.Id == kingdom.Id)
            return ServiceResult.Fail("Nie można zawrzeć paktu z samym sobą.");
        if (target.CoalitionId != kingdom.CoalitionId)
            return ServiceResult.Fail("Partner musi należeć do Twojej koalicji.");

        if (!AllPactTypes.Contains(dto.PactType))
            return ServiceResult.Fail("Nieznany typ paktu.");

        // Istniejący pakt TEGO TYPU z tym partnerem (rekord wspólny w obu kierunkach).
        var existing = await _context.Pacts.FirstOrDefaultAsync(p => p.Status == "Active" &&
            p.PactType == dto.PactType &&
            ((p.ProposerKingdomId == kingdom.Id && p.TargetKingdomId == target.Id) ||
             (p.ProposerKingdomId == target.Id && p.TargetKingdomId == kingdom.Id)));

        if (!dto.Active)
        {
            // Zerwanie paktu danego typu.
            if (existing == null)
                return ServiceResult.Ok($"Nie masz paktu {dto.PactType.ToLower()} z księstwem {target.Name}.");
            _context.Pacts.Remove(existing);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"Zerwano pakt {dto.PactType.ToLower()} z księstwem {target.Name}.");
        }

        // Zawarcie paktu danego typu.
        if (existing != null)
            return ServiceResult.Ok($"Pakt {dto.PactType.ToLower()} z księstwem {target.Name} już obowiązuje.");

        // Limit łącznej liczby paktów (wszystkich typów).
        int used = await _context.Pacts.CountAsync(p => p.Status == "Active" &&
            (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id));
        int limit = LimitFor(kingdom);
        if (used >= limit)
            return ServiceResult.Fail(
                $"Osiągnięto limit {limit} paktów. Zbuduj Ambasadę, aby zwiększyć limit.");

        _context.Pacts.Add(new Pact
        {
            ProposerKingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            PactType = dto.PactType,
            Status = "Active",
            ConfirmedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zawarto pakt {dto.PactType.ToLower()} z księstwem {target.Name}.");
    }

    /// <summary>Aktywni partnerzy paktów obronnych danego typu dla księstwa.</summary>
    public static async Task<List<Kingdom>> GetActivePactPartnersAsync(
        ApplicationDbContext context, int kingdomId, string pactType)
    {
        var pacts = await context.Pacts
            .Where(p => p.PactType == pactType && p.Status == "Active"
                        && (p.ProposerKingdomId == kingdomId || p.TargetKingdomId == kingdomId))
            .ToListAsync();

        var partnerIds = pacts
            .Select(p => p.ProposerKingdomId == kingdomId ? p.TargetKingdomId : p.ProposerKingdomId)
            .Distinct()
            .ToList();

        return await context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .Where(k => partnerIds.Contains(k.Id))
            .ToListAsync();
    }
}
