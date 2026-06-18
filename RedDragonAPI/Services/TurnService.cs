using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public class TurnService : ITurnService
{
    private readonly ApplicationDbContext _context;
    private readonly IResourceService _resourceService;
    private readonly IGeneralService _generalService;

    public TurnService(ApplicationDbContext context, IResourceService resourceService, IGeneralService generalService)
    {
        _context = context;
        _resourceService = resourceService;
        _generalService = generalService;
    }

    public async Task<TurnResultDto> UseTurnAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return new TurnResultDto { Success = false, Message = "Nie znaleziono księstwa." };

        if (kingdom.IsFrozen)
            return new TurnResultDto { Success = false, Message = "Księstwo jest zamrożone — odmróź je, aby działać." };

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

        // Postęp budynku specjalnego i bieżącego badania (do pokazania przyrostu % w Stolicy)
        int beforeSpecialProgress = kingdom.SpecialBuildingProgress;
        string? beforeTech = kingdom.CurrentResearchTech;
        long beforeInvested = beforeTech == null ? 0 : await _context.Researches.AsNoTracking()
            .Where(r => r.KingdomId == kingdom.Id && r.TechType == beforeTech)
            .Select(r => (long?)r.InvestedScience).FirstOrDefaultAsync() ?? 0;

        kingdom.TurnsAvailable--;
        kingdom.Age++;
        kingdom.LastActive = DateTime.UtcNow;

        // Generuj zasoby za tę turę
        await _resourceService.GenerateResourcesForKingdomAsync(kingdom);

        // Generałowie zdobywają doświadczenie z każdą turą (pomijamy będących na wyprawie,
        // uwięzionych i oczekujących w poczekalni). Wg Dracopedii (§11) generałów szkolą
        // naukowcy — bazę 150 powiększa liczba naukowców (do +600), więc inwestycja w naukę
        // przyspiesza rozwój dowódców.
        int scientistCount = kingdom.Professions
            .FirstOrDefault(p => p.ProfessionType == "Naukowcy")?.WorkerCount ?? 0;
        int generalExpPerTurn = 150 + Math.Min(600, scientistCount);
        var homeGenerals = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id && !g.IsPending && !g.IsOutside && !g.IsImprisoned)
            .ToListAsync();
        foreach (var gen in homeGenerals)
            gen.Experience += generalExpPerTurn;

        await _context.SaveChangesAsync();

        // Próba przyjścia generała (gwarantowana, gdy księstwo nie ma żadnego generała)
        await _generalService.TryGeneralArrivalAsync(kingdom);

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

        // Przyrost postępu budynku specjalnego — tylko gdy budowa nadal trwa (po ukończeniu zerowane).
        if (!string.IsNullOrEmpty(kingdom.CurrentSpecialBuilding))
            deltas["specialBuildingProgress"] = kingdom.SpecialBuildingProgress - beforeSpecialProgress;

        // Przyrost postępu badania — tylko gdy ta sama dziedzina nadal w toku.
        if (beforeTech != null && kingdom.CurrentResearchTech == beforeTech)
        {
            long nowInvested = await _context.Researches.AsNoTracking()
                .Where(r => r.KingdomId == kingdom.Id && r.TechType == beforeTech)
                .Select(r => (long?)r.InvestedScience).FirstOrDefaultAsync() ?? 0;
            deltas["researchProgress"] = nowInvested - beforeInvested;
        }

        return new TurnResultDto
        {
            Success = true,
            Message = $"Tura wykorzystana. Pozostało tur: {kingdom.TurnsAvailable}",
            TurnsRemaining = kingdom.TurnsAvailable,
            Deltas = deltas
        };
    }
}
