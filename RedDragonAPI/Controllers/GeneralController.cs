using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeneralController : ControllerBase
{
    private readonly IGeneralService _generalService;

    public GeneralController(IGeneralService generalService)
    {
        _generalService = generalService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetGenerals()
    {
        return Ok(await _generalService.GetGeneralsAsync(UserId));
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var result = await _generalService.AcceptGeneralAsync(UserId, id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Dismiss(int id)
    {
        var result = await _generalService.DismissGeneralAsync(UserId, id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
