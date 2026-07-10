using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

/// <summary>
/// Badania wg manuala (docs/MECHANIKA.md §13, docs/zrodla/manual-pl/vyzkum.txt):
/// dziedziny rozwijane Punktami Nauki (SP) produkowanymi przez naukowców.
/// Gracz wybiera dziedzinę, w którą inwestowana jest nadprodukcja SP (jak budowa
/// budynku specjalnego); zmiana niedokończonej dziedziny kosztuje 1/3 zainwestowanych SP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResearchController : ControllerBase
{
    private static readonly long[] DevelopmentCaps = { 20_000, 35_000, 50_000, 100_000, 130_000, 150_000 };

    private readonly ApplicationDbContext _context;

    public ResearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<TechDefinitionDto>>> GetAvailableTechnologies()
    {
        var kingdom = await LoadKingdomAsync();
        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var definitions = await _context.TechnologyDefinitions.OrderBy(d => d.Id).ToListAsync();

        var result = definitions.Select(d =>
        {
            var research = kingdom.Researches.FirstOrDefault(r => r.TechType == d.TechType);
            var (canResearch, reason) = CheckCanResearch(kingdom, d);
            bool isCompleted = research?.IsCompleted ?? false;
            bool isCurrent = kingdom.CurrentResearchTech == d.TechType;

            return new TechDefinitionDto
            {
                Id = d.Id,
                TechType = d.TechType,
                Category = d.Category,
                DisplayName = d.DisplayName,
                Description = d.Description,
                CostScience = d.CostScience,
                InvestedScience = research?.InvestedScience ?? 0,
                IsCurrent = isCurrent,
                RequiredTech = d.RequiredTech,
                RequiredBuilding = d.RequiredBuilding,
                EffectType = d.EffectType,
                EffectValue = d.EffectValue,
                IsCompleted = isCompleted,
                CanResearch = canResearch && !isCompleted && !isCurrent,
                CannotResearchReason = isCompleted ? "Już zbadane"
                    : isCurrent ? "Rozwijane teraz"
                    : reason
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("status")]
    public async Task<ActionResult<ResearchStatusDto>> GetStatus()
    {
        var kingdom = await LoadKingdomAsync();
        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        int devLevel = kingdom.Researches.Count(r => r.IsCompleted && r.TechType.StartsWith("Wynalazki"));
        long cap = DevelopmentCaps[Math.Min(devLevel, DevelopmentCaps.Length - 1)];
        decimal mult = kingdom.Race switch { "Człowiek" => 1.33m, "Goblin" => 0.8m, _ => 1m };

        return Ok(new ResearchStatusDto
        {
            SciencePoints = kingdom.SciencePoints,
            CurrentResearchTech = kingdom.CurrentResearchTech,
            SciencePerTurnCap = (long)(cap * mult)
        });
    }

    [HttpGet("my-research")]
    public async Task<ActionResult<List<ResearchDto>>> GetMyResearch()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var result = kingdom.Researches.Select(r => new ResearchDto
        {
            Id = r.Id,
            TechType = r.TechType,
            DisplayName = r.Tech?.DisplayName ?? r.TechType,
            Category = r.Tech?.Category ?? "",
            Description = r.Tech?.Description,
            IsCompleted = r.IsCompleted,
            IsInProgress = r.IsInProgress
        }).ToList();

        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<ActionResult> StartResearch([FromBody] StartResearchDto dto)
    {
        var kingdom = await LoadKingdomAsync();
        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var techDef = await _context.TechnologyDefinitions
            .FirstOrDefaultAsync(t => t.TechType == dto.TechType);
        if (techDef == null)
            return BadRequest("Nieznana dziedzina.");

        var existing = kingdom.Researches.FirstOrDefault(r => r.TechType == dto.TechType);
        if (existing != null && existing.IsCompleted)
            return BadRequest("Już zbadane.");
        if (kingdom.CurrentResearchTech == dto.TechType)
            return BadRequest("Ta dziedzina jest już rozwijana.");

        var (canResearch, reason) = CheckCanResearch(kingdom, techDef);
        if (!canResearch)
            return BadRequest(reason);

        // Zmiana niedokończonej dziedziny: tracimy 1/3 zainwestowanych SP w poprzedniej
        if (!string.IsNullOrEmpty(kingdom.CurrentResearchTech))
        {
            var previous = kingdom.Researches.FirstOrDefault(r =>
                r.TechType == kingdom.CurrentResearchTech && !r.IsCompleted);
            if (previous != null)
            {
                previous.InvestedScience = previous.InvestedScience * 2 / 3;
                previous.IsInProgress = false;
            }
        }

        // Ustaw bieżącą dziedzinę (utwórz rekord badania, jeśli nie istnieje)
        if (existing == null)
        {
            existing = new Research
            {
                KingdomId = kingdom.Id,
                TechType = dto.TechType,
                IsInProgress = true,
                InvestedScience = 0
            };
            _context.Researches.Add(existing);
        }
        else
        {
            existing.IsInProgress = true; // wznawiamy (zachowuje dotychczasowe SP)
        }

        kingdom.CurrentResearchTech = dto.TechType;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult
        {
            Success = true,
            Message = $"Naukowcy rozwijają teraz: {techDef.DisplayName}. " +
                      $"Potrzeba {techDef.CostScience:N0} Punktów Nauki."
        });
    }

    private async Task<Kingdom?> LoadKingdomAsync()
    {
        var userId = GetUserId();
        return await _context.Kingdoms
            .Include(k => k.Researches)
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
    }

    private (bool canResearch, string? reason) CheckCanResearch(
        Kingdom kingdom, TechnologyDefinition techDef)
    {
        // W Red Dragon badania prowadzą naukowcy (profesja) inwestując Punkty Nauki —
        // nie ma wymogu budynku „uniwersytetu". Wymagane są jedynie wcześniejsze dziedziny.
        // Ent (Dracopedia/Inżynieria): poziomy 4 i 5 niedostępne.
        if (kingdom.Race == "Ent" && techDef.TechType is "Inzynieria4" or "Inzynieria5")
            return (false, "Enty nie mają dostępu do zaawansowanej Inżynierii.");

        if (!string.IsNullOrEmpty(techDef.RequiredTech))
        {
            var prereq = kingdom.Researches.FirstOrDefault(r => r.TechType == techDef.RequiredTech && r.IsCompleted);
            if (prereq == null)
                return (false, $"Wymaga technologii: {techDef.RequiredTech}");
        }

        if (!string.IsNullOrEmpty(techDef.RequiredBuilding))
        {
            var reqBuilding = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == techDef.RequiredBuilding && b.Quantity > 0);
            if (reqBuilding == null)
                return (false, $"Wymaga budynku: {techDef.RequiredBuilding}");
        }

        return (true, null);
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}
