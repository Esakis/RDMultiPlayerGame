using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DragonController : ControllerBase
{
    private readonly IDragonService _dragonService;
    private readonly IBattleService _battleService;

    public DragonController(IDragonService dragonService, IBattleService battleService)
    {
        _dragonService = dragonService;
        _battleService = battleService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _dragonService.GetStatusAsync(UserId);
        return status == null ? NotFound("Nie znaleziono księstwa.") : Ok(status);
    }

    /// <summary>Przywołanie smoka wprost z widoku Smoków (rzuca zaklęcie Przywołanie Smoka).</summary>
    [HttpPost("summon")]
    public async Task<IActionResult> Summon()
    {
        var result = await _battleService.CastSpellAsync(UserId,
            new CastSpellDto { SpellType = "PrzywolanieSmoka" });
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
