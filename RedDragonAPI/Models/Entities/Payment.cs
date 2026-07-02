using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Opłata za księstwo. KingdomId celowo bez klucza obcego — historia płatności
/// ma przetrwać usunięcie księstwa (np. skasowanego po 30 dniach bez opłaty).
/// </summary>
[Table("Payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int KingdomId { get; set; }

    /// <summary>Nazwa księstwa w chwili płatności (księstwo może zostać później usunięte).</summary>
    [Required, MaxLength(200)]
    public string KingdomName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>Metoda płatności: BLIK | Karta | Przelew.</summary>
    [Required, MaxLength(50)]
    public string Method { get; set; } = string.Empty;

    /// <summary>Completed — płatności symulowane są księgowane od razu.</summary>
    [Required, MaxLength(50)]
    public string Status { get; set; } = "Completed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
