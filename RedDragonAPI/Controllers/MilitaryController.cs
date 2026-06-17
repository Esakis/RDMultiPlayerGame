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

    [HttpPost("recruit-batch")]
    public async Task<ActionResult> RecruitBatch([FromBody] UnitBatchDto dto)
    {
        var result = await _militaryService.RecruitBatchAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("disband")]
    public async Task<ActionResult> Disband([FromBody] UnitBatchDto dto)
    {
        var result = await _militaryService.DisbandBatchAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("training")]
    public async Task<ActionResult<TrainingInfoDto>> GetTraining()
    {
        return Ok(await _militaryService.GetTrainingInfoAsync(GetUserId()));
    }

    [HttpPost("training")]
    public async Task<ActionResult> SetTraining([FromBody] SetTrainingDto dto)
    {
        var result = await _militaryService.SetTrainingAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
