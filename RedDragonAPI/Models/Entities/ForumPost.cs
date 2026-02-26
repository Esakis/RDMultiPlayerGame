using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("ForumPosts")]
public class ForumPost
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ForumType { get; set; } = "General"; // "General" or "Coalition"

    public int? CoalitionId { get; set; }

    public int AuthorKingdomId { get; set; }

    [MaxLength(50)]
    public string? AuthorCoalitionRole { get; set; }

    [MaxLength(50)]
    public string? SubForum { get; set; } // "Ważne" or "Pogawędki" for coalition forum

    [Required]
    public string Body { get; set; } = string.Empty;

    public int? ParentPostId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Kingdom AuthorKingdom { get; set; } = null!;
    public Coalition? Coalition { get; set; }
    public ForumPost? ParentPost { get; set; }
    public ICollection<ForumPost> Replies { get; set; } = new List<ForumPost>();
}
