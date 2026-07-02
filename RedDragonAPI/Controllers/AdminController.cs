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
