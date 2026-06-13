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

    [HttpPost("enter")]
    public async Task<IActionResult> Enter([FromBody] EnterLabyrinthDto dto)
    {
        var result = await _labyrinthService.EnterAsync(UserId, dto.GeneralId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("advance")]
    public async Task<IActionResult> Advance()
    {
        var result = await _labyrinthService.AdvanceAsync(UserId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("retreat")]
    public async Task<IActionResult> Retreat()
    {
        var result = await _labyrinthService.RetreatAsync(UserId);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
