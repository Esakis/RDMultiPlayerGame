using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabyrinthController : ControllerBase
{
    private readonly ILabyrinthService _labyrinthService;

    public LabyrinthController(ILabyrinthService labyrinthService)
    {
        _labyrinthService = labyrinthService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        return Ok(await _labyrinthService.GetStatusAsync(UserId));
    }

    [HttpPost("treasure")]
    public async Task<IActionResult> TakeTreasure([FromBody] TakeTreasureDto dto)
    {
        var result = await _labyrinthService.TakeTreasureAsync(UserId, dto.GeneralId, dto.TreasureType);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("search-general")]
    public async Task<IActionResult> SearchGeneral([FromBody] GeneralActionDto dto)
    {
        var result = await _labyrinthService.SearchGeneralAsync(UserId, dto.GeneralId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("change-ability")]
    public async Task<IActionResult> ChangeAbility([FromBody] GeneralActionDto dto)
    {
        var result = await _labyrinthService.ChangeAbilityAsync(UserId, dto.GeneralId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
