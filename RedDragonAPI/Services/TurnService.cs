using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public class TurnService : ITurnService
{
    private readonly ApplicationDbContext _context;
    private readonly IResourceService _resourceService;

    public TurnService(ApplicationDbContext context, IResourceService resourceService)
    {
        _context = context;
        _resourceService = resourceService;
    }

    public async Task<ServiceResult> UseTurnAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult.Fail("Brak dostępnych tur.");

        kingdom.TurnsAvailable--;
        kingdom.Age++;
        kingdom.LastActive = DateTime.UtcNow;

        // Generuj zasoby za tę turę
        await _resourceService.GenerateResourcesForKingdomAsync(kingdom);

        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"Tura wykorzystana. Pozostało tur: {kingdom.TurnsAvailable}");
    }
}
