using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Dziennik zdarzeń księstwa odnotowanych podczas przeliczenia
/// (ukończone budowy, wyszkolone jednostki itp.). Wyświetlany na Stolicy.
/// </summary>
[Table("KingdomEvents")]
public class KingdomEvent
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;
}
