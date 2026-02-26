namespace RedDragonAPI.Models.DTOs;

public class ForumPostDto
{
    public int Id { get; set; }
    public string ForumType { get; set; } = string.Empty;
    public int? CoalitionId { get; set; }
    public int AuthorKingdomId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCoalitionTag { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? ParentPostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReplyCount { get; set; }
    public List<ForumPostDto> Replies { get; set; } = new();
}

public class CreateForumPostDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? ParentPostId { get; set; }
}
