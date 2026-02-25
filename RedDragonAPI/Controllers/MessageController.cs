using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessageController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("inbox")]
    public async Task<ActionResult<List<MessageDto>>> GetInbox()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var messages = await _context.Messages
            .Where(m => m.ReceiverKingdomId == kingdom.Id)
            .Include(m => m.SenderKingdom)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderName = m.SenderKingdom.Name,
                ReceiverName = kingdom.Name,
                Subject = m.Subject,
                Body = m.Body,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<MessageDto>>> GetSent()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var messages = await _context.Messages
            .Where(m => m.SenderKingdomId == kingdom.Id)
            .Include(m => m.ReceiverKingdom)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderName = kingdom.Name,
                ReceiverName = m.ReceiverKingdom.Name,
                Subject = m.Subject,
                Body = m.Body,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<ActionResult> Send([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var receiver = await _context.Kingdoms.FindAsync(dto.ReceiverKingdomId);
        if (receiver == null)
            return NotFound("Nie znaleziono odbiorcy.");

        var message = new Message
        {
            SenderKingdomId = kingdom.Id,
            ReceiverKingdomId = dto.ReceiverKingdomId,
            Subject = dto.Subject,
            Body = dto.Body,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Wiadomość wysłana." });
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound();

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.ReceiverKingdomId == kingdom.Id);

        if (message == null)
            return NotFound();

        message.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok();
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
