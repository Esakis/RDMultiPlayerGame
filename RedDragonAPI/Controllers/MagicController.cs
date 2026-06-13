using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MagicController : ControllerBase
{
    private readonly IBattleService _battleService;

    public MagicController(IBattleService battleService)
    {
        _battleService = battleService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("spells")]
    public async Task<IActionResult> GetSpells()
    {
        var spells = await _battleService.GetAvailableSpellsAsync(UserId);
        return Ok(spells);
    }

    [HttpPost("cast")]
    public async Task<IActionResult> Cast([FromBody] CastSpellDto dto)
    {
        var result = await _battleService.CastSpellAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
