using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Pakt między księstwami tej samej koalicji (oryginalne 4 typy, docs/MECHANIKA.md §12).
/// Limit 5 paktów na księstwo (+1 z Ambasadą); z jednym księstwem tylko jeden pakt
/// danego typu; obie strony muszą potwierdzić.
/// Skuteczność obronna: 1 pakt = 50%, 2 = 45%, 3+ = 40% (na typ).
/// </summary>
[Table("Pacts")]
public class Pact
{
    [Key]
    public int Id { get; set; }

    public int ProposerKingdomId { get; set; }
    public int TargetKingdomId { get; set; }

    /// <summary>Handlowy | Magiczny | Wojskowy | Zlodziejski</summary>
    [Required, MaxLength(30)]
    public string PactType { get; set; } = string.Empty;

    /// <summary>Proposed | Active | Cancelled</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Proposed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public Kingdom ProposerKingdom { get; set; } = null!;
    public Kingdom TargetKingdom { get; set; } = null!;
}
