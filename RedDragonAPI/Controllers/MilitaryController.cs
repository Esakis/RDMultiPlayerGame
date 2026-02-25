using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MilitaryController : ControllerBase
{
    private readonly IMilitaryService _militaryService;

    public MilitaryController(IMilitaryService militaryService)
    {
        _militaryService = militaryService;
    }

    [HttpGet("available-units")]
    public async Task<ActionResult<List<UnitDefinitionDto>>> GetAvailableUnits()
    {
        var userId = GetUserId();
        var units = await _militaryService.GetAvailableUnitsAsync(userId);
        return Ok(units);
    }

    [HttpGet("my-army")]
    public async Task<ActionResult<List<MilitaryUnitDto>>> GetMyArmy()
    {
        var userId = GetUserId();
        var army = await _militaryService.GetMyArmyAsync(userId);
        return Ok(army);
    }

    [HttpPost("recruit")]
    public async Task<ActionResult> Recruit([FromBody] RecruitUnitsDto dto)
    {
        var userId = GetUserId();
        var result = await _militaryService.RecruitUnitsAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
