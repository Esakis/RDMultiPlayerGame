using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BattleController : ControllerBase
{
    private readonly IBattleService _battleService;

    public BattleController(IBattleService battleService)
    {
        _battleService = battleService;
    }

    [HttpPost("attack")]
    public async Task<ActionResult> Attack([FromBody] AttackDto dto)
    {
        var userId = GetUserId();
        var result = await _battleService.QueueAttackAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("planned")]
    public async Task<ActionResult<List<PlannedAttackDto>>> GetPlannedAttacks()
    {
        var userId = GetUserId();
        return Ok(await _battleService.GetPlannedAttacksAsync(userId));
    }

    [HttpDelete("planned/{id:int}")]
    public async Task<ActionResult> CancelPlannedAttack(int id)
    {
        var userId = GetUserId();
        var result = await _battleService.CancelPlannedAttackAsync(userId, id);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<List<BattleReportDto>>> GetReports()
    {
        var userId = GetUserId();
        var reports = await _battleService.GetBattleReportsAsync(userId);
        return Ok(reports);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
