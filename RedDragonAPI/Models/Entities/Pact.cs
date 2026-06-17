using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Pakt OBRONNY między księstwami tej samej koalicji (Magiczny | Wojskowy | Zlodziejski).
/// Pakt handlowy jest domyślny dla każdego współczłonka i NIE ma tu rekordu — istnienie
/// rekordu oznacza, że domyślny handlowy zastąpiono paktem obronnym. Jeden rekord na parę
/// księstw (wspólny dla obu stron), tworzony natychmiast (Status="Active", bez akceptacji).
/// Limit paktów obronnych: 5 na księstwo (+1 z Ambasadą). Szczegóły: docs/zrodla/urza-pakt.txt.
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
