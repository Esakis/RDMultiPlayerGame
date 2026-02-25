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
public class CoalitionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoalitionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<CoalitionDto>>> GetCoalitions([FromQuery] int? eraId)
    {
        int era = eraId ?? 1;
        var coalitions = await _context.Coalitions
            .Where(c => c.EraId == era)
            .Include(c => c.Members)
            .Include(c => c.Leader)
            .Select(c => new CoalitionDto
            {
                Id = c.Id,
                Name = c.Name,
                Tag = c.Tag,
                LeaderKingdomId = c.LeaderKingdomId,
                LeaderName = c.Leader != null ? c.Leader.Name : null,
                MemberCount = c.Members.Count,
                MaxMembers = c.MaxMembers,
                PSOProgress = c.PSOProgress,
                Members = c.Members.Select(m => new KingdomSummaryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Race = m.Race,
                    Land = m.Land,
                    Population = m.Population
                }).ToList()
            })
            .ToListAsync();

        return Ok(coalitions);
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateCoalitionDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        if (kingdom.CoalitionId.HasValue)
            return BadRequest("Już należysz do koalicji.");

        var coalition = new Coalition
        {
            Name = dto.Name,
            Tag = dto.Tag,
            LeaderKingdomId = kingdom.Id,
            EraId = kingdom.EraId,
            MaxMembers = 17
        };

        _context.Coalitions.Add(coalition);
        await _context.SaveChangesAsync();

        kingdom.CoalitionId = coalition.Id;
        kingdom.CoalitionRole = "Leader";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Koalicja '{dto.Name}' została utworzona." });
    }

    [HttpPost("join")]
    public async Task<ActionResult> Join([FromBody] JoinCoalitionDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        if (kingdom.CoalitionId.HasValue)
            return BadRequest("Już należysz do koalicji.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == dto.CoalitionId);

        if (coalition == null)
            return NotFound("Nie znaleziono koalicji.");

        if (coalition.Members.Count >= coalition.MaxMembers)
            return BadRequest("Koalicja jest pełna.");

        kingdom.CoalitionId = coalition.Id;
        kingdom.CoalitionRole = "Member";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Dołączono do koalicji '{coalition.Name}'." });
    }

    [HttpPost("leave")]
    public async Task<ActionResult> Leave()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        if (!kingdom.CoalitionId.HasValue)
            return BadRequest("Nie należysz do żadnej koalicji.");

        var coalition = await _context.Coalitions
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition != null && coalition.LeaderKingdomId == kingdom.Id)
        {
            // Lider opuszcza - wyznacz nowego lub rozwiąż
            var newLeader = await _context.Kingdoms
                .Where(k => k.CoalitionId == coalition.Id && k.Id != kingdom.Id)
                .FirstOrDefaultAsync();

            if (newLeader != null)
            {
                coalition.LeaderKingdomId = newLeader.Id;
                newLeader.CoalitionRole = "Leader";
            }
            else
            {
                _context.Coalitions.Remove(coalition);
            }
        }

        kingdom.CoalitionId = null;
        kingdom.CoalitionRole = null;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Opuszczono koalicję." });
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
