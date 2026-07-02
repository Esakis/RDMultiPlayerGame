using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

/// <summary>
/// Opłaty za księstwa. Płatności są symulowane (BLIK / Karta / Przelew) —
/// zaksięgowanie następuje natychmiast, bez zewnętrznej bramki.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private static readonly string[] AllowedMethods = { "BLIK", "Karta", "Przelew" };

    private readonly ApplicationDbContext _context;

    public PaymentController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Aktualna opłata za założenie płatnego księstwa.</summary>
    [HttpGet("price")]
    public async Task<ActionResult<KingdomPriceDto>> GetPrice()
    {
        return Ok(new KingdomPriceDto { Price = await PaymentRules.GetKingdomPriceAsync(_context) });
    }

    /// <summary>Opłać księstwo — zdejmuje zawieszenie i chroni przed usunięciem.</summary>
    [HttpPost("pay")]
    public async Task<ActionResult<PaymentDto>> Pay([FromBody] PayForKingdomDto dto)
    {
        var userId = GetUserId();

        if (!AllowedMethods.Contains(dto.Method))
            return BadRequest("Nieznana metoda płatności. Dostępne: BLIK, Karta, Przelew.");

        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.KingdomId && k.UserId == userId);
        if (kingdom == null)
            return NotFound("Nie znaleziono takiego księstwa na tym koncie.");

        if (kingdom.IsPaid)
            return BadRequest("To księstwo jest już opłacone.");
        if (kingdom.IsFree)
            return BadRequest("To księstwo jest darmowe — nie wymaga opłaty.");
        if (kingdom.CoalitionRole == "Imperator")
            return BadRequest("Księstwo imperatorskie jest zawsze darmowe — nie wymaga opłaty.");

        var price = await PaymentRules.GetKingdomPriceAsync(_context);

        var payment = new Payment
        {
            UserId = userId,
            KingdomId = kingdom.Id,
            KingdomName = kingdom.Name,
            Amount = price,
            Method = dto.Method,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);

        kingdom.IsPaid = true;
        kingdom.PaidAt = DateTime.UtcNow;
        kingdom.IsSuspended = false;

        await _context.SaveChangesAsync();

        return Ok(ToDto(payment, null));
    }

    /// <summary>Historia płatności konta.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<PaymentDto>>> GetHistory()
    {
        var userId = GetUserId();
        var payments = await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments.Select(p => ToDto(p, null)).ToList());
    }

    internal static PaymentDto ToDto(Payment p, string? username) => new()
    {
        Id = p.Id,
        KingdomId = p.KingdomId,
        KingdomName = p.KingdomName,
        Amount = p.Amount,
        Method = p.Method,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        Username = username
    };

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
