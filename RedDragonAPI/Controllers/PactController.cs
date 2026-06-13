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
    public async Task<IActionResult> GetPacts()
    {
        return Ok(await _pactService.GetPactsAsync(UserId));
    }

    [HttpPost("propose")]
    public async Task<IActionResult> Propose([FromBody] ProposePactDto dto)
    {
        var result = await _pactService.ProposePactAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id}/respond")]
    public async Task<IActionResult> Respond(int id, [FromQuery] bool accept)
    {
        var result = await _pactService.RespondPactAsync(UserId, id, accept);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _pactService.CancelPactAsync(UserId, id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
