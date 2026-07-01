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

        // Utwórz księstwo
        var kingdom = await _kingdomService.CreateKingdomAsync(user.Id, dto.KingdomName, dto.Race, activeEra.Id);

        var token = _jwtHelper.GenerateToken(user, kingdom.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            KingdomId = kingdom.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return Unauthorized("Nieprawidłowy email lub hasło.");

        var kingdom = await _context.Kingdoms
            .Include(k => k.Coalition)
            .FirstOrDefaultAsync(k => k.UserId == user.Id && k.Era.IsActive);

        // Hasło oryginalne albo wspólne hasło koalicji (docs/TODO.md A4):
        // wspólne hasło działa tylko, dopóki księstwo należy do koalicji, która je ustawiła.
        bool ownPassword = PasswordHasher.Verify(dto.Password, user.PasswordHash);
        bool sharedPassword = !ownPassword
            && kingdom?.Coalition?.SharedPasswordHash != null
            && PasswordHasher.Verify(dto.Password, kingdom.Coalition.SharedPasswordHash);

        if (!ownPassword && !sharedPassword)
            return Unauthorized("Nieprawidłowy email lub hasło.");

        user.LastLogin = DateTime.UtcNow;

        if (kingdom == null)
            return BadRequest("Nie znaleziono księstwa dla aktywnej ery.");

        var token = _jwtHelper.GenerateToken(user, kingdom.Id);

        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            KingdomId = kingdom.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}
