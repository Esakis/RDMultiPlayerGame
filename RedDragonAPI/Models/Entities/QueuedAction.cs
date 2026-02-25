using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("QueuedActions")]
public class QueuedAction
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }
    public int? TargetKingdomId { get; set; }

    [Required, MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    public string? ActionData { get; set; }

    public DateTime ScheduledFor { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }

    public Kingdom Kingdom { get; set; } = null!;
    public Kingdom? TargetKingdom { get; set; }
}
