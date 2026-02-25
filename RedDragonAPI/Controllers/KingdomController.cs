using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KingdomController : ControllerBase
{
    private readonly IKingdomService _kingdomService;
    private readonly ITurnService _turnService;

    public KingdomController(IKingdomService kingdomService, ITurnService turnService)
    {
        _kingdomService = kingdomService;
        _turnService = turnService;
    }

    [HttpGet("my-kingdom")]
    public async Task<ActionResult<KingdomDto>> GetMyKingdom()
    {
        var userId = GetUserId();
        var kingdom = await _kingdomService.GetKingdomByUserIdAsync(userId);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        return Ok(kingdom);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<KingdomDto>> GetKingdom(int id)
    {
        var kingdom = await _kingdomService.GetKingdomByIdAsync(id);

        if (kingdom == null)
            return NotFound();

        return Ok(kingdom);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<KingdomSummaryDto>>> GetAllKingdoms([FromQuery] int? eraId)
    {
        int era = eraId ?? 1;
        var kingdoms = await _kingdomService.GetAllKingdomsAsync(era);
        return Ok(kingdoms);
    }

    [HttpPost("use-turn")]
    public async Task<ActionResult> UseTurn()
    {
        var userId = GetUserId();
        var result = await _turnService.UseTurnAsync(userId);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("assign-workers")]
    public async Task<ActionResult> AssignWorkers([FromBody] AssignWorkersDto dto)
    {
        var userId = GetUserId();
        var result = await _kingdomService.AssignWorkersAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("buy-land")]
    public async Task<ActionResult> BuyLand([FromBody] BuyLandDto dto)
    {
        var userId = GetUserId();
        var result = await _kingdomService.BuyLandAsync(userId, dto.Amount);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
