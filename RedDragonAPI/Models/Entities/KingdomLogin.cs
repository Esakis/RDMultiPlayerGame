using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Wpis logowania do księstwa (przy logowaniu i przełączeniu księstwa) —
/// adres IP widoczny w panelu super admina.
/// </summary>
[Table("KingdomLogins")]
public class KingdomLogin
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(64)]
    public string IpAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;
}
