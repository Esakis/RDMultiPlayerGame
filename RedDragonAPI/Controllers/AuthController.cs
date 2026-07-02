using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;
using RedDragonAPI.Services;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IKingdomService _kingdomService;

    public AuthController(ApplicationDbContext context, JwtHelper jwtHelper, IKingdomService kingdomService)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _kingdomService = kingdomService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email jest już zajęty.");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Nazwa użytkownika jest już zajęta.");

        var user = new User
        {
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Znajdź aktywną erę
        var activeEra = await _context.Eras.FirstOrDefaultAsync(e => e.IsActive);
        if (activeEra == null)
            return BadRequest("Brak aktywnej ery.");

        // Utwórz księstwo — pierwsze księstwo konta jest darmowe
        var kingdom = await _kingdomService.CreateKingdomAsync(user.Id, dto.KingdomName, dto.Race, activeEra.Id);
        kingdom.IsFree = true;
        user.ActiveKingdomId = kingdom.Id;
        _context.KingdomLogins.Add(new KingdomLogin
        {
            UserId = user.Id,
            KingdomId = kingdom.Id,
            IpAddress = RequestHelper.GetClientIp(HttpContext)
        });
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

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return Unauthorized("Nieprawidłowy email lub hasło.");

        // Konto może mieć wiele księstw — logujemy do aktywnie wybranego,
        // a gdy go brak (albo jest zawieszone), do pierwszego dostępnego.
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Coalition)
            .Where(k => k.UserId == user.Id && k.Era.IsActive)
            .OrderBy(k => k.Id)
            .ToListAsync();

        var kingdom = kingdoms.FirstOrDefault(k => k.Id == user.ActiveKingdomId && !k.IsSuspended && !k.IsPaymentOverdue)
            ?? kingdoms.FirstOrDefault(k => !k.IsSuspended && !k.IsPaymentOverdue);

        // Hasło oryginalne albo wspólne hasło koalicji (docs/TODO.md A4):
        // wspólne hasło działa tylko, dopóki księstwo należy do koalicji, która je ustawiła.
        bool ownPassword = PasswordHasher.Verify(dto.Password, user.PasswordHash);
        bool sharedPassword = !ownPassword
            && kingdom?.Coalition?.SharedPasswordHash != null
            && PasswordHasher.Verify(dto.Password, kingdom.Coalition.SharedPasswordHash);

        if (!ownPassword && !sharedPassword)
            return Unauthorized("Nieprawidłowy email lub hasło.");

        user.LastLogin = DateTime.UtcNow;

        // kingdom == null jest dozwolone: admin bez księstwa, konto z samymi
        // zawieszonymi księstwami (musi móc się zalogować, żeby zapłacić) albo
        // konto po usunięciu księstw. Token dostaje wtedy KingdomId = 0,
        // a klient kieruje na ekran wyboru/opłacenia księstwa.
        user.ActiveKingdomId = kingdom?.Id;

        if (kingdom != null)
        {
            _context.KingdomLogins.Add(new KingdomLogin
            {
                UserId = user.Id,
                KingdomId = kingdom.Id,
                IpAddress = RequestHelper.GetClientIp(HttpContext)
            });
        }

        var token = _jwtHelper.GenerateToken(user, kingdom?.Id ?? 0);

        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            KingdomId = kingdom?.Id ?? 0,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}
