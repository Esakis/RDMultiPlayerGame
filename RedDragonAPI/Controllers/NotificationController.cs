using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;

namespace RedDragonAPI.Controllers;

/// <summary>
/// Lekki endpoint statusu dla nagłówka gry (docs/TODO.md C1): licznik nieprzeczytanych
/// wiadomości, liczba raportów z ostatniego przeliczenia i czas następnego przeliczenia.
/// Klient odpytuje go cyklicznie (polling).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
        if (kingdom == null) return Ok(new { unreadMessages = 0, reportsSinceReset = 0, nextResetAt = (DateTime?)null });

        // Granice przeliczenia: codziennie 5:00 czasu serwera (jak DailyResetService)
        var nowLocal = DateTime.Now;
        var lastResetLocal = nowLocal.Hour >= 5
            ? nowLocal.Date.AddHours(5)
            : nowLocal.Date.AddDays(-1).AddHours(5);
        var nextResetLocal = lastResetLocal.AddDays(1);
        var lastResetUtc = lastResetLocal.ToUniversalTime();

        int unreadMessages = await _context.Messages.CountAsync(m =>
            m.ReceiverKingdomId == kingdom.Id && !m.IsRead);

        int reportsSinceReset = await _context.BattleReports.CountAsync(b =>
            (b.AttackerKingdomId == kingdom.Id || b.DefenderKingdomId == kingdom.Id)
            && b.OccurredAt >= lastResetUtc);

        return Ok(new
        {
            unreadMessages,
            reportsSinceReset,
            nextResetAt = nextResetLocal.ToUniversalTime(),
            serverTimeUtc = DateTime.UtcNow
        });
    }
}
