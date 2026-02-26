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
                    Population = m.Population,
                    CoalitionRole = m.CoalitionRole
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
        kingdom.CoalitionRole = "Imperator";
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
                newLeader.CoalitionRole = "Imperator";
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

    [HttpPost("appoint-main-commander")]
    public async Task<ActionResult<ServiceResult>> AppointMainCommander([FromBody] AppointMainCommanderDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może mianować Głównodowodzącego.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        var targetKingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.KingdomId && k.CoalitionId == coalition.Id);

        if (targetKingdom == null) return NotFound("Wybrane księstwo nie należy do tej koalicji.");
        if (targetKingdom.Id == kingdom.Id) return BadRequest("Nie możesz mianować siebie.");

        // Remove existing MainCommander if any
        var existingCommander = coalition.Members.FirstOrDefault(m => m.CoalitionRole == "MainCommander");
        if (existingCommander != null)
        {
            existingCommander.CoalitionRole = "Member";
        }

        // Appoint new MainCommander
        targetKingdom.CoalitionRole = "MainCommander";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Mianowano {targetKingdom.Name} na Głównodowodzącego." });
    }

    [HttpPost("remove-main-commander")]
    public async Task<ActionResult<ServiceResult>> RemoveMainCommander()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może usunąć Głównodowodzącego.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        var commander = coalition.Members.FirstOrDefault(m => m.CoalitionRole == "MainCommander");
        if (commander == null) return BadRequest("Nie ma Głównodowodzącego w koalicji.");

        commander.CoalitionRole = "Member";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Usunięto {commander.Name} z funkcji Głównodowodzącego." });
    }

    private async Task<Kingdom?> GetCurrentKingdom()
    {
        var userId = GetUserId();
        return await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
