using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

/// <summary>Panel super admina — m.in. ustawianie opłaty za księstwo (domyślnie 30 zł).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("kingdom-price")]
    public async Task<ActionResult<KingdomPriceDto>> GetKingdomPrice()
    {
        return Ok(new KingdomPriceDto { Price = await PaymentRules.GetKingdomPriceAsync(_context) });
    }

    [HttpPut("kingdom-price")]
    public async Task<ActionResult<KingdomPriceDto>> SetKingdomPrice([FromBody] KingdomPriceDto dto)
    {
        if (dto.Price < 0)
            return BadRequest("Opłata nie może być ujemna.");

        var setting = await _context.GameSettings
            .FirstOrDefaultAsync(s => s.Key == GameSetting.KingdomPriceKey);

        if (setting == null)
        {
            setting = new GameSetting { Key = GameSetting.KingdomPriceKey };
            _context.GameSettings.Add(setting);
        }

        setting.Value = dto.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await _context.SaveChangesAsync();

        return Ok(new KingdomPriceDto { Price = dto.Price });
    }

    /// <summary>Wszystkie księstwa aktywnej ery (panel blokad).</summary>
    [HttpGet("kingdoms")]
    public async Task<ActionResult<List<AdminKingdomDto>>> GetKingdoms()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.User)
            .Where(k => k.Era.IsActive)
            .OrderBy(k => k.Id)
            .ToListAsync();

        return Ok(kingdoms.Select(k =>
        {
            var dto = PaymentRules.ToAccountDto(k, null);
            return new AdminKingdomDto
            {
                Id = k.Id,
                Name = k.Name,
                Username = k.User.Username,
                Race = k.Race,
                Land = k.Land,
                Age = k.Age,
                Status = dto.Status,
                IsSuspended = dto.IsSuspended,
                AdminLocked = k.AdminLocked,
                IsFree = k.IsFree,
                IsPaid = k.IsPaid,
                DaysSinceCreation = dto.DaysSinceCreation
            };
        }).ToList());
    }

    /// <summary>Ręcznie zablokuj księstwo (niezależnie od opłat).</summary>
    [HttpPost("kingdoms/{id:int}/lock")]
    public async Task<ActionResult> LockKingdom(int id)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == id);
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");

        kingdom.AdminLocked = true;
        kingdom.IsSuspended = true;
        if (kingdom.User.ActiveKingdomId == kingdom.Id)
            kingdom.User.ActiveKingdomId = null;

        _context.KingdomEvents.Add(new KingdomEvent
        {
            KingdomId = kingdom.Id,
            Category = "Admin",
            Message = "Księstwo zostało zablokowane przez administratora."
        });

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>Zdejmij ręczną blokadę (zawieszenie za brak opłaty pozostaje w mocy).</summary>
    [HttpPost("kingdoms/{id:int}/unlock")]
    public async Task<ActionResult> UnlockKingdom(int id)
    {
        var kingdom = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == id);
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");

        kingdom.AdminLocked = false;
        kingdom.IsSuspended = kingdom.IsPaymentOverdue;

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>Adresy IP, z jakich logowano się do księstwa (zgrupowane, najnowsze pierwsze).</summary>
    [HttpGet("kingdoms/{id:int}/logins")]
    public async Task<ActionResult<List<KingdomLoginDto>>> GetKingdomLogins(int id)
    {
        var logins = await _context.KingdomLogins
            .Where(l => l.KingdomId == id)
            .GroupBy(l => l.IpAddress)
            .Select(g => new KingdomLoginDto
            {
                Ip = g.Key,
                Count = g.Count(),
                LastAt = g.Max(l => l.CreatedAt)
            })
            .OrderByDescending(l => l.LastAt)
            .ToListAsync();

        return Ok(logins);
    }

    /// <summary>Wszystkie płatności w grze (najnowsze pierwsze).</summary>
    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetAllPayments()
    {
        var payments = await _context.Payments
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Take(200)
            .ToListAsync();

        return Ok(payments.Select(p => PaymentController.ToDto(p, p.User.Username)).ToList());
    }
}
