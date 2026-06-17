using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PactController : ControllerBase
{
    private readonly IPactService _pactService;

    public PactController(IPactService pactService)
    {
        _pactService = pactService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        return Ok(await _pactService.GetPactStatusAsync(UserId));
    }

    [HttpPost("set")]
    public async Task<IActionResult> Set([FromBody] SetPactDto dto)
    {
        var result = await _pactService.SetPactAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
