using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Wyprawa generała do labiryntu (docs/MECHANIKA.md §13).
/// Minigra typu „push your luck": generał schodzi coraz głębiej (koszt: 1 tura/poziom),
/// zbiera łupy i kości, ale ryzyko pułapki (rana) i potworów (śmierć) rośnie z głębią.
/// Łup gromadzi się w wyprawie (Pending*) i jest deponowany do skarbca dopiero przy
/// wycofaniu lub bezpiecznym zakończeniu; przy śmierci generała przepada.
/// Elf zbiera 1,5× łupów materialnych.
/// </summary>
[Table("LabyrinthExpeditions")]
public class LabyrinthExpedition
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    /// <summary>Generał prowadzący wyprawę; null, gdy zginął w labiryncie (FK SetNull).</summary>
    public int? GeneralId { get; set; }

    /// <summary>Obecna głębokość (liczba pokonanych poziomów).</summary>
    public int Depth { get; set; } = 0;

    /// <summary>Active | Ended</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";

    // Łup zgromadzony, jeszcze niezdeponowany
    public long PendingGold { get; set; }
    public long PendingFood { get; set; }
    public long PendingStone { get; set; }
    public long PendingWeapons { get; set; }
    public long PendingMana { get; set; }
    public int PendingDice { get; set; }

    [MaxLength(300)]
    public string? LastEvent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;
    public General? General { get; set; }
}
