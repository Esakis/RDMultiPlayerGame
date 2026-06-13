using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DragonController : ControllerBase
{
    private readonly IDragonService _dragonService;

    public DragonController(IDragonService dragonService)
    {
        _dragonService = dragonService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _dragonService.GetStatusAsync(UserId);
        return status == null ? NotFound("Nie znaleziono księstwa.") : Ok(status);
    }
}
