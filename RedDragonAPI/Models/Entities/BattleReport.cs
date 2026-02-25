using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("BattleReports")]
public class BattleReport
{
    [Key]
    public int Id { get; set; }

    public int AttackerKingdomId { get; set; }
    public int DefenderKingdomId { get; set; }

    [Required, MaxLength(50)]
    public string BattleType { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Result { get; set; } = string.Empty;

    public string? AttackerLosses { get; set; }
    public string? DefenderLosses { get; set; }
    public string? ResourcesStolen { get; set; }
    public int LandCaptured { get; set; } = 0;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Kingdom AttackerKingdom { get; set; } = null!;
    public Kingdom DefenderKingdom { get; set; } = null!;
}
