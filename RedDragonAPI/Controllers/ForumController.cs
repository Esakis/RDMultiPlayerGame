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
public class ForumController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ForumController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("general")]
    public async Task<ActionResult<List<ForumPostDto>>> GetGeneralPosts()
    {
        var posts = await _context.ForumPosts
            .Where(f => f.ForumType == "General" && f.ParentPostId == null)
            .OrderByDescending(f => f.CreatedAt)
            .Take(100)
            .Include(f => f.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .Include(f => f.Replies)
            .ThenInclude(r => r.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .ToListAsync();

        return Ok(posts.Select(MapToDto).ToList());
    }

    [HttpGet("coalition")]
    public async Task<ActionResult<List<ForumPostDto>>> GetCoalitionPosts()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");

        var posts = await _context.ForumPosts
            .Where(f => f.ForumType == "Coalition" && f.CoalitionId == kingdom.CoalitionId && f.ParentPostId == null)
            .OrderByDescending(f => f.CreatedAt)
            .Take(100)
            .Include(f => f.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .Include(f => f.Replies)
            .ThenInclude(r => r.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .ToListAsync();

        return Ok(posts.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ForumPostDto>> GetPost(int id)
    {
        var post = await _context.ForumPosts
            .Include(f => f.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .Include(f => f.Replies)
            .ThenInclude(r => r.AuthorKingdom)
            .ThenInclude(k => k.Coalition)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (post == null) return NotFound();

        return Ok(MapToDto(post));
    }

    [HttpPost("general")]
    public async Task<ActionResult<ForumPostDto>> CreateGeneralPost([FromBody] CreateForumPostDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");

        var post = new ForumPost
        {
            ForumType = "General",
            AuthorKingdomId = kingdom.Id,
            Subject = dto.Subject,
            Body = dto.Body,
            ParentPostId = dto.ParentPostId
        };

        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();

        await _context.Entry(post).Reference(f => f.AuthorKingdom).LoadAsync();
        return Ok(MapToDto(post));
    }

    [HttpPost("coalition")]
    public async Task<ActionResult<ForumPostDto>> CreateCoalitionPost([FromBody] CreateForumPostDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");

        var post = new ForumPost
        {
            ForumType = "Coalition",
            CoalitionId = kingdom.CoalitionId,
            AuthorKingdomId = kingdom.Id,
            Subject = dto.Subject,
            Body = dto.Body,
            ParentPostId = dto.ParentPostId
        };

        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();

        await _context.Entry(post).Reference(f => f.AuthorKingdom).LoadAsync();
        return Ok(MapToDto(post));
    }

    private async Task<Kingdom?> GetCurrentKingdom()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        return await _context.Kingdoms
            .Include(k => k.Coalition)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
    }

    private ForumPostDto MapToDto(ForumPost post)
    {
        return new ForumPostDto
        {
            Id = post.Id,
            ForumType = post.ForumType,
            CoalitionId = post.CoalitionId,
            AuthorKingdomId = post.AuthorKingdomId,
            AuthorName = post.AuthorKingdom?.Name ?? "???",
            AuthorCoalitionTag = post.AuthorKingdom?.Coalition?.Tag,
            Subject = post.Subject,
            Body = post.Body,
            ParentPostId = post.ParentPostId,
            CreatedAt = post.CreatedAt,
            ReplyCount = post.Replies?.Count ?? 0,
            Replies = post.Replies?.OrderBy(r => r.CreatedAt).Select(MapToDto).ToList() ?? new()
        };
    }
}
