using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IPactService
{
    Task<List<PactDto>> GetPactsAsync(int userId);
    Task<ServiceResult> ProposePactAsync(int userId, ProposePactDto dto);
    Task<ServiceResult> RespondPactAsync(int userId, int pactId, bool accept);
    Task<ServiceResult> CancelPactAsync(int userId, int pactId);
}

/// <summary>
/// Pakty wg oryginału (docs/MECHANIKA.md §12): 4 typy, tylko w obrębie koalicji,
/// limit 5 na księstwo, jeden pakt danego typu z jednym księstwem,
/// obustronne potwierdzenie.
/// </summary>
public class PactService : IPactService
{
    public const int PactLimit = 5; // +1 z Ambasadą (budynek do dodania)

    public static readonly string[] PactTypes =
        { "Handlowy", "Magiczny", "Wojskowy", "Zlodziejski" };

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
        await _context.Kingdoms.FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

    public async Task<List<PactDto>> GetPactsAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null) return new List<PactDto>();

        return await _context.Pacts
            .Where(p => (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id)
                        && p.Status != "Cancelled")
            .Include(p => p.ProposerKingdom)
            .Include(p => p.TargetKingdom)
            .Select(p => new PactDto
            {
                Id = p.Id,
                PactType = p.PactType,
                Status = p.Status,
                PartnerKingdomId = p.ProposerKingdomId == kingdom.Id ? p.TargetKingdomId : p.ProposerKingdomId,
                PartnerName = p.ProposerKingdomId == kingdom.Id ? p.TargetKingdom.Name : p.ProposerKingdom.Name,
                IsProposer = p.ProposerKingdomId == kingdom.Id,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ServiceResult> ProposePactAsync(int userId, ProposePactDto dto)
    {
        if (!PactTypes.Contains(dto.PactType))
            return ServiceResult.Fail("Nieznany typ paktu.");

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

        int myPacts = await _context.Pacts.CountAsync(p =>
            (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id) && p.Status == "Active");
        if (myPacts >= PactLimit)
            return ServiceResult.Fail($"Osiągnięto limit {PactLimit} paktów.");

        bool duplicate = await _context.Pacts.AnyAsync(p =>
            p.PactType == dto.PactType && p.Status != "Cancelled" &&
            ((p.ProposerKingdomId == kingdom.Id && p.TargetKingdomId == target.Id) ||
             (p.ProposerKingdomId == target.Id && p.TargetKingdomId == kingdom.Id)));
        if (duplicate)
            return ServiceResult.Fail("Taki pakt z tym księstwem już istnieje lub czeka na potwierdzenie.");

        _context.Pacts.Add(new Pact
        {
            ProposerKingdomId = kingdom.Id,
            TargetKingdomId = target.Id,
            PactType = dto.PactType,
            Status = "Proposed"
        });
        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"Zaproponowano pakt {dto.PactType.ToLower()} księstwu {target.Name}.");
    }

    public async Task<ServiceResult> RespondPactAsync(int userId, int pactId, bool accept)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var pact = await _context.Pacts
            .FirstOrDefaultAsync(p => p.Id == pactId && p.TargetKingdomId == kingdom.Id && p.Status == "Proposed");
        if (pact == null)
            return ServiceResult.Fail("Nie znaleziono oczekującej propozycji paktu.");

        if (!accept)
        {
            pact.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Propozycja paktu odrzucona.");
        }

        int myPacts = await _context.Pacts.CountAsync(p =>
            (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id) && p.Status == "Active");
        if (myPacts >= PactLimit)
            return ServiceResult.Fail($"Osiągnięto limit {PactLimit} paktów.");

        pact.Status = "Active";
        pact.ConfirmedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Pakt zawarty.");
    }

    public async Task<ServiceResult> CancelPactAsync(int userId, int pactId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var pact = await _context.Pacts.FirstOrDefaultAsync(p => p.Id == pactId
            && (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id)
            && p.Status != "Cancelled");
        if (pact == null)
            return ServiceResult.Fail("Nie znaleziono paktu.");

        pact.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Pakt wypowiedziany.");
    }

    /// <summary>Aktywni partnerzy paktów danego typu dla księstwa.</summary>
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
