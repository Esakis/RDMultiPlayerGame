using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Messages")]
public class Message
{
    [Key]
    public int Id { get; set; }

    public int SenderKingdomId { get; set; }
    public int ReceiverKingdomId { get; set; }

    [MaxLength(500)]
    public string? Subject { get; set; }

    public string? Body { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Kingdom SenderKingdom { get; set; } = null!;
    public Kingdom ReceiverKingdom { get; set; } = null!;
}
