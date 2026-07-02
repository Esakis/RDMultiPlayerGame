using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

/// <summary>
/// Zarządzanie księstwami konta: konto może mieć wiele księstw, pierwsze jest
/// darmowe, kolejne wymagają opłaty (do 20. dnia, inaczej zawieszenie; usunięcie po 30).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IKingdomService _kingdomService;
    private readonly JwtHelper _jwtHelper;

    public AccountController(ApplicationDbContext context, IKingdomService kingdomService, JwtHelper jwtHelper)
    {
        _context = context;
        _kingdomService = kingdomService;
        _jwtHelper = jwtHelper;
    }

    /// <summary>Lista księstw konta ze statusem opłat.</summary>
    [HttpGet("kingdoms")]
    public async Task<ActionResult<List<AccountKingdomDto>>> GetMyKingdoms()
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var kingdoms = await _context.Kingdoms
            .Where(k => k.UserId == userId && k.Era.IsActive)
            .OrderBy(k => k.Id)
            .ToListAsync();

        return Ok(kingdoms.Select(k => PaymentRules.ToAccountDto(k, user.ActiveKingdomId)).ToList());
    }

    /// <summary>Załóż kolejne księstwo (płatne — do opłacenia w ciągu 20 dni).</summary>
    [HttpPost("kingdoms")]
    public async Task<ActionResult<AccountKingdomDto>> CreateKingdom([FromBody] CreateKingdomDto dto)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var activeEra = await _context.Eras.FirstOrDefaultAsync(e => e.IsActive);
        if (activeEra == null)
            return BadRequest("Brak aktywnej ery.");

        if (await _context.Kingdoms.AnyAsync(k => k.EraId == activeEra.Id && k.Name == dto.Name))
            return BadRequest("Księstwo o tej nazwie już istnieje w tej erze.");

        var kingdom = await _kingdomService.CreateKingdomAsync(userId, dto.Name, dto.Race, activeEra.Id);

        // Pierwsze księstwo konta jest darmowe — kolejne wymagają opłaty.
        kingdom.IsFree = !await _context.Kingdoms
            .AnyAsync(k => k.UserId == userId && k.Id != kingdom.Id);
        await _context.SaveChangesAsync();

        return Ok(PaymentRules.ToAccountDto(kingdom, user.ActiveKingdomId));
    }

    /// <summary>Wybierz księstwo do gry — zwraca nowy token z jego kontekstem.</summary>
    [HttpPost("select/{kingdomId:int}")]
    public async Task<ActionResult<AuthResponseDto>> SelectKingdom(int kingdomId)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == kingdomId && k.UserId == userId && k.Era.IsActive);
        if (kingdom == null)
            return NotFound("Nie znaleziono takiego księstwa na tym koncie.");

        // Egzekwuj termin płatności także między przeliczeniami dziennymi.
        if (kingdom.IsPaymentOverdue && !kingdom.IsSuspended)
        {
            kingdom.IsSuspended = true;
            await _context.SaveChangesAsync();
        }

        if (kingdom.IsSuspended)
            return BadRequest($"Księstwo jest zawieszone za brak opłaty. Opłać je, aby grać " +
                $"(po {Kingdom.DeletionDays} dniach od założenia zostanie usunięte).");

        user.ActiveKingdomId = kingdom.Id;
        await _context.SaveChangesAsync();

        var token = _jwtHelper.GenerateToken(user, kingdom.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            KingdomId = kingdom.Id,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
