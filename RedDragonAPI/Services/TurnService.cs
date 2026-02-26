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

    public async Task<TurnResultDto> UseTurnAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return new TurnResultDto { Success = false, Message = "Nie znaleziono księstwa." };

        if (kingdom.TurnsAvailable <= 0)
            return new TurnResultDto { Success = false, Message = "Brak dostępnych tur." };

        // Snapshot before
        var before = new Dictionary<string, long>
        {
            ["gold"] = kingdom.Gold,
            ["food"] = kingdom.Food,
            ["stone"] = kingdom.Stone,
            ["budulecStored"] = kingdom.BudulecStored,
            ["weapons"] = kingdom.Weapons,
            ["mana"] = kingdom.Mana,
            ["population"] = kingdom.Population,
            ["popularity"] = kingdom.Popularity
        };

        kingdom.TurnsAvailable--;
        kingdom.Age++;
        kingdom.LastActive = DateTime.UtcNow;

        // Generuj zasoby za tę turę
        await _resourceService.GenerateResourcesForKingdomAsync(kingdom);

        await _context.SaveChangesAsync();

        // Calculate deltas
        var deltas = new Dictionary<string, long>
        {
            ["gold"] = kingdom.Gold - before["gold"],
            ["food"] = kingdom.Food - before["food"],
            ["stone"] = kingdom.Stone - before["stone"],
            ["budulecStored"] = kingdom.BudulecStored - before["budulecStored"],
            ["weapons"] = kingdom.Weapons - before["weapons"],
            ["mana"] = kingdom.Mana - before["mana"],
            ["population"] = kingdom.Population - before["population"],
            ["popularity"] = kingdom.Popularity - before["popularity"]
        };

        return new TurnResultDto
        {
            Success = true,
            Message = $"Tura wykorzystana. Pozostało tur: {kingdom.TurnsAvailable}",
            TurnsRemaining = kingdom.TurnsAvailable,
            Deltas = deltas
        };
    }
}
