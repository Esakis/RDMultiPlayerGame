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
/// z KAŻDYM współczłonkiem masz domyślnie pakt handlowy (bez rekordu w bazie).
/// Zmiana typu na obronny (Magiczny/Wojskowy/Zlodziejski) tworzy rekord, który
/// zastępuje handlowy dla tego sojusznika i zajmuje slot. Powrót na handlowy =
/// usunięcie rekordu. Pakty są natychmiastowe (jednostronne), rekord jest wspólny
/// dla obu stron (symetryczny efekt obronny i zajmuje slot u obu).
/// Limit paktów obronnych: baza 5 + Ambasada (+1).
/// Skuteczność obronna wg liczby paktów danego typu: 50%/45%/40%.
/// </summary>
public class PactService : IPactService
{
    public const int BasePactLimit = 5; // +1 z Ambasadą
    public const string TradePactType = "Handlowy";

    /// <summary>Typy paktów obronnych (handlowy jest stanem domyślnym, nie obronnym).</summary>
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

        var typeByPartner = pacts.ToDictionary(
            p => p.ProposerKingdomId == kingdom.Id ? p.TargetKingdomId : p.ProposerKingdomId,
            p => p.PactType);
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
                PactType = typeByPartner.TryGetValue(m.Id, out var t) ? t : TradePactType
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

        bool isTrade = dto.PactType == TradePactType;
        if (!isTrade && !DefensePactTypes.Contains(dto.PactType))
            return ServiceResult.Fail("Nieznany typ paktu.");

        // Istniejący pakt obronny z tym partnerem (rekord jest wspólny, w obu kierunkach).
        var existing = await _context.Pacts.FirstOrDefaultAsync(p => p.Status == "Active" &&
            ((p.ProposerKingdomId == kingdom.Id && p.TargetKingdomId == target.Id) ||
             (p.ProposerKingdomId == target.Id && p.TargetKingdomId == kingdom.Id)));

        if (isTrade)
        {
            // Powrót do domyślnego paktu handlowego = usunięcie paktu obronnego.
            if (existing == null)
                return ServiceResult.Ok($"Z księstwem {target.Name} masz już pakt handlowy.");
            _context.Pacts.Remove(existing);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"Przywrócono domyślny pakt handlowy z księstwem {target.Name}.");
        }

        if (existing != null)
        {
            if (existing.PactType == dto.PactType)
                return ServiceResult.Ok($"Pakt {dto.PactType.ToLower()} z księstwem {target.Name} już obowiązuje.");
            // Zmiana typu istniejącego paktu obronnego nie zajmuje nowego slotu.
            existing.PactType = dto.PactType;
            existing.ConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ServiceResult.Ok($"Zmieniono pakt z księstwem {target.Name} na {dto.PactType.ToLower()}.");
        }

        // Nowy pakt obronny — sprawdź limit.
        int used = await _context.Pacts.CountAsync(p => p.Status == "Active" &&
            (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id));
        int limit = LimitFor(kingdom);
        if (used >= limit)
            return ServiceResult.Fail(
                $"Osiągnięto limit {limit} paktów obronnych. Zbuduj Ambasadę, aby zwiększyć limit.");

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
