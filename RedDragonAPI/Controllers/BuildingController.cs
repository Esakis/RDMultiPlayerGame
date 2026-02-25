using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<BuildingDefinitionDto>>> GetAvailableBuildings()
    {
        var userId = GetUserId();
        var buildings = await _buildingService.GetAvailableBuildingsAsync(userId);
        return Ok(buildings);
    }

    [HttpGet("my-buildings")]
    public async Task<ActionResult<List<BuildingDto>>> GetMyBuildings()
    {
        var userId = GetUserId();
        var buildings = await _buildingService.GetMyBuildingsAsync(userId);
        return Ok(buildings);
    }

    [HttpPost("construct")]
    public async Task<ActionResult> Construct([FromBody] ConstructBuildingDto dto)
    {
        var userId = GetUserId();
        var result = await _buildingService.ConstructBuildingAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
