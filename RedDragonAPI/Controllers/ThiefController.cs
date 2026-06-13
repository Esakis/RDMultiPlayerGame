using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ThiefController : ControllerBase
{
    private readonly IBattleService _battleService;

    public ThiefController(IBattleService battleService)
    {
        _battleService = battleService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("actions")]
    public async Task<IActionResult> GetActions()
    {
        var actions = await _battleService.GetThiefActionsAsync();
        return Ok(actions);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendThievesDto dto)
    {
        var result = await _battleService.SendThievesAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
