using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<TechDefinitionDto>>> GetAvailableTechnologies()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .Include(k => k.Researches)
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var hasUniversity = kingdom.Buildings.Any(b => b.BuildingType == "University" && b.Quantity > 0);

        var definitions = await _context.TechnologyDefinitions.ToListAsync();

        var result = definitions.Select(d =>
        {
            var research = kingdom.Researches.FirstOrDefault(r => r.TechType == d.TechType);
            var (canResearch, reason) = CheckCanResearch(kingdom, d, hasUniversity);

            return new TechDefinitionDto
            {
                Id = d.Id,
                TechType = d.TechType,
                Category = d.Category,
                DisplayName = d.DisplayName,
                Description = d.Description,
                CostGold = d.CostGold,
                ResearchTime = d.ResearchTime,
                RequiredTech = d.RequiredTech,
                RequiredBuilding = d.RequiredBuilding,
                EffectType = d.EffectType,
                EffectValue = d.EffectValue,
                CanResearch = canResearch && research == null,
                CannotResearchReason = research != null
                    ? (research.IsCompleted ? "Już zbadane" : "W trakcie badania")
                    : reason
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("my-research")]
    public async Task<ActionResult<List<ResearchDto>>> GetMyResearch()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .Include(k => k.Researches).ThenInclude(r => r.Tech)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

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
            IsInProgress = r.IsInProgress,
            CompletesAt = r.CompletesAt
        }).ToList();

        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<ActionResult> StartResearch([FromBody] StartResearchDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .Include(k => k.Researches)
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var techDef = await _context.TechnologyDefinitions
            .FirstOrDefaultAsync(t => t.TechType == dto.TechType);

        if (techDef == null)
            return BadRequest("Nieznana technologia.");

        // Sprawdź czy już badane
        var existing = kingdom.Researches.FirstOrDefault(r => r.TechType == dto.TechType);
        if (existing != null)
            return BadRequest(existing.IsCompleted ? "Już zbadane." : "Badanie już w toku.");

        // Sprawdź czy jest inny research w toku
        if (kingdom.Researches.Any(r => r.IsInProgress))
            return BadRequest("Możesz prowadzić tylko jedno badanie na raz.");

        var hasUniversity = kingdom.Buildings.Any(b => b.BuildingType == "University" && b.Quantity > 0);
        var (canResearch, reason) = CheckCanResearch(kingdom, techDef, hasUniversity);

        if (!canResearch)
            return BadRequest(reason);

        if (kingdom.Gold < techDef.CostGold)
            return BadRequest($"Za mało złota. Potrzeba: {techDef.CostGold}");

        kingdom.Gold -= techDef.CostGold;

        var research = new Research
        {
            KingdomId = kingdom.Id,
            TechType = dto.TechType,
            IsInProgress = true,
            CompletesAt = DateTime.UtcNow.AddDays(techDef.ResearchTime)
        };

        _context.Researches.Add(research);
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult
        {
            Success = true,
            Message = $"Rozpoczęto badanie {techDef.DisplayName}. Ukończenie za {techDef.ResearchTime} dni."
        });
    }

    private (bool canResearch, string? reason) CheckCanResearch(
        Kingdom kingdom, TechnologyDefinition techDef, bool hasUniversity)
    {
        if (!hasUniversity)
            return (false, "Wymaga Uniwersytetu.");

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

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
