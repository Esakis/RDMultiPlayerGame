using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MagicController : ControllerBase
{
    private readonly IBattleService _battleService;
    private readonly ApplicationDbContext _context;

    public MagicController(IBattleService battleService, ApplicationDbContext context)
    {
        _battleService = battleService;
        _context = context;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("spells")]
    public async Task<IActionResult> GetSpells()
    {
        var spells = await _battleService.GetAvailableSpellsAsync(UserId);
        return Ok(spells);
    }

    [HttpPost("cast")]
    public async Task<IActionResult> Cast([FromBody] CastSpellDto dto)
    {
        var result = await _battleService.CastSpellAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    /// <summary>Aktualne zaklęcie auto-rzucane po przeliczeniu (null = wyłączone).</summary>
    [HttpGet("auto-cast")]
    public async Task<IActionResult> GetAutoCast()
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == UserId && k.Era.IsActive);
        return Ok(new { spellType = kingdom?.AutoCastSpellType });
    }

    /// <summary>
    /// Ustawia zaklęcie auto-rzucane na siebie po każdym przeliczeniu (kosztuje turę i manę).
    /// Dozwolone tylko pozytywne zaklęcia na własne księstwo; null/puste wyłącza.
    /// </summary>
    [HttpPost("auto-cast")]
    public async Task<IActionResult> SetAutoCast([FromBody] SetAutoCastDto dto)
    {
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == UserId && k.Era.IsActive);
        if (kingdom == null) return BadRequest("Nie znaleziono księstwa.");

        if (string.IsNullOrWhiteSpace(dto.SpellType))
        {
            kingdom.AutoCastSpellType = null;
            await _context.SaveChangesAsync();
            return Ok(new ServiceResult { Success = true, Message = "Auto-rzucanie wyłączone." });
        }

        var spell = await _context.SpellDefinitions
            .FirstOrDefaultAsync(s => s.SpellType == dto.SpellType);
        if (spell == null) return BadRequest("Nieznane zaklęcie.");
        if (spell.TargetType == "Enemy" || spell.Category is not ("Biała" or "Tarcze"))
            return BadRequest("Automatycznie można rzucać tylko pozytywne zaklęcia na własne księstwo.");

        var race = await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == kingdom.Race);
        bool available = spell.RequiredRace != null
            ? spell.RequiredRace == kingdom.Race
            : (race?.MagicBooks ?? 0) >= spell.RequiredBooks;
        if (!available) return BadRequest("Twoja rasa nie zna tego zaklęcia.");

        kingdom.AutoCastSpellType = spell.SpellType;
        await _context.SaveChangesAsync();
        return Ok(new ServiceResult
        {
            Success = true,
            Message = $"Po każdym przeliczeniu automatycznie rzucisz: {spell.DisplayName} (koszt: tura + mana)."
        });
    }
}
