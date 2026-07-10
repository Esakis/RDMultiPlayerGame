using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IPactService
{
    Task<PactStatusDto> GetPactStatusAsync(int userId);
    Task<ServiceResult> SetPactAsync(int userId, SetPactDto dto);
    Task<ServiceResult> SetTradePactAsync(int userId, bool enabled);
}

/// <summary>
/// Pakty wewnątrz koalicji (docs/zrodla/urza-pakt.txt + decyzje projektowe 2026-07-10):
/// - Pakty OBRONNE (Magiczny | Wojskowy | Zlodziejski) wskazują KONKRETNEGO partnera
///   z koalicji — po jednym pakcie danego typu z jednym księstwem. Wartości obronne
///   (magowie/armia/złodzieje) pochodzą od wskazanego partnera. Limit: 5 + Ambasada.
/// - Pakt HANDLOWY (kupiecki) nie wskazuje partnera: to udział w wymianie handlowej
///   koalicji (kupcy liczą ziemię współczłonków z włączonym handlem). Poza limitem.
/// - PAKTY POŁÓWKOWE: świeżo zawarty pakt (obronny lub handlowy) do najbliższego
///   przeliczenia (5:00) działa z POŁOWĄ wartości; po przeliczeniu — pełną.
/// Skuteczność obronna wg liczby paktów danego typu: 50%/45%/40%.
/// </summary>
public class PactService : IPactService
{
    public const int BasePactLimit = 5; // +1 z Ambasadą
    public const string TradePactType = "Handlowy";

    /// <summary>Typy paktów obronnych (per partner, wspomagają obronę).</summary>
    public static readonly string[] DefensePactTypes =
        { "Magiczny", "Wojskowy", "Zlodziejski" };

    /// <summary>Skuteczność paktu obronnego wg liczby paktów danego typu: 50%/45%/40%.</summary>
    public static decimal PactEfficiency(int pactCountOfType) => pactCountOfType switch
    {
        <= 1 => 0.50m,
        2 => 0.45m,
        _ => 0.40m
    };

    /// <summary>
    /// Początek bieżącej doby gry (ostatnie przeliczenie o 5:00 czasu serwera) w UTC.
    /// Pakty zawarte po tym momencie działają połowicznie do następnego przeliczenia.
    /// </summary>
    public static DateTime LastResetUtc()
    {
        var nowLocal = DateTime.Now;
        var lastResetLocal = nowLocal.Hour >= 5
            ? nowLocal.Date.AddHours(5)
            : nowLocal.Date.AddDays(-1).AddHours(5);
        return lastResetLocal.ToUniversalTime();
    }

    /// <summary>Czy pakt zawarty w danym momencie jest jeszcze połówkowy.</summary>
    public static bool IsHalf(DateTime? confirmedAtUtc) =>
        confirmedAtUtc.HasValue && confirmedAtUtc.Value >= LastResetUtc();

    private readonly ApplicationDbContext _context;

    public PactService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Kingdom?> GetKingdomAsync(int userId) =>
        await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

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
            InCoalition = kingdom.CoalitionId != null,
            TradePactEnabled = kingdom.TradePactEnabled,
            TradePactHalf = kingdom.TradePactEnabled && IsHalf(kingdom.TradePactSince)
        };
        if (kingdom.CoalitionId == null) return dto;

        var pacts = await _context.Pacts
            .Where(p => p.Status == "Active"
                        && (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id))
            .ToListAsync();

        // Aktywne typy paktów per partner + które z nich są jeszcze połówkowe.
        var byPartner = pacts
            .GroupBy(p => p.ProposerKingdomId == kingdom.Id ? p.TargetKingdomId : p.ProposerKingdomId)
            .ToDictionary(g => g.Key, g => g.ToList());
        dto.UsedSlots = pacts.Count;

        var members = await _context.Kingdoms
            .Where(k => k.CoalitionId == kingdom.CoalitionId && k.Id != kingdom.Id)
            .Select(k => new { k.Id, k.Name, k.Race, k.Land, k.TradePactEnabled })
            .ToListAsync();

        dto.Members = members
            .Select(m =>
            {
                var partnerPacts = byPartner.TryGetValue(m.Id, out var list) ? list : new List<Pact>();
                return new PactMemberDto
                {
                    KingdomId = m.Id,
                    Name = m.Name,
                    Race = m.Race,
                    Land = m.Land,
                    TradePactEnabled = m.TradePactEnabled,
                    ActivePacts = partnerPacts.Select(p => p.PactType).Distinct().ToList(),
                    HalfPacts = partnerPacts.Where(p => IsHalf(p.ConfirmedAt))
                        .Select(p => p.PactType).Distinct().ToList()
                };
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

        if (dto.PactType == TradePactType)
            return ServiceResult.Fail("Pakt handlowy nie wskazuje partnera — włącz go przełącznikiem handlu.");
        if (!DefensePactTypes.Contains(dto.PactType))
            return ServiceResult.Fail("Nieznany typ paktu.");

        var target = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == dto.TargetKingdomId);
        if (target == null)
            return ServiceResult.Fail("Nie znaleziono księstwa partnera.");
        if (target.Id == kingdom.Id)
            return ServiceResult.Fail("Nie można zawrzeć paktu z samym sobą.");
        if (target.CoalitionId != kingdom.CoalitionId)
            return ServiceResult.Fail("Partner musi należeć do Twojej koalicji.");

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

        // Limit łącznej liczby paktów obronnych.
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
        return ServiceResult.Ok(
            $"Zawarto pakt {dto.PactType.ToLower()} z księstwem {target.Name}. " +
            "Do najbliższego przeliczenia działa z połową wartości.");
    }

    public async Task<ServiceResult> SetTradePactAsync(int userId, bool enabled)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null)
            return ServiceResult.Fail("Pakt handlowy działa tylko w obrębie koalicji.");

        if (kingdom.TradePactEnabled == enabled)
            return ServiceResult.Ok(enabled ? "Pakt handlowy już obowiązuje." : "Pakt handlowy nie jest zawarty.");

        kingdom.TradePactEnabled = enabled;
        kingdom.TradePactSince = enabled ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok(enabled
            ? "Zawarto pakt handlowy — kupcy skorzystają z ziemi współczłonków z włączonym handlem. Do najbliższego przeliczenia działa z połową wartości."
            : "Zerwano pakt handlowy.");
    }

    /// <summary>
    /// Aktywni partnerzy paktów obronnych danego typu dla księstwa
    /// wraz z flagą paktu połówkowego (zawarty po ostatnim przeliczeniu).
    /// </summary>
    public static async Task<List<(Kingdom Partner, bool Half)>> GetActivePactPartnersAsync(
        ApplicationDbContext context, int kingdomId, string pactType)
    {
        var pacts = await context.Pacts
            .Where(p => p.PactType == pactType && p.Status == "Active"
                        && (p.ProposerKingdomId == kingdomId || p.TargetKingdomId == kingdomId))
            .ToListAsync();

        var halfByPartner = pacts
            .GroupBy(p => p.ProposerKingdomId == kingdomId ? p.TargetKingdomId : p.ProposerKingdomId)
            .ToDictionary(g => g.Key, g => g.Any(p => IsHalf(p.ConfirmedAt)));

        var partners = await context.Kingdoms
            .Include(k => k.MilitaryUnits).ThenInclude(m => m.Definition)
            .Include(k => k.Professions)
            .Where(k => halfByPartner.Keys.Contains(k.Id))
            .ToListAsync();

        return partners.Select(k => (k, halfByPartner[k.Id])).ToList();
    }
}
