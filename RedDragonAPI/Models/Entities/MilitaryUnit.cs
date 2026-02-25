using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("MilitaryUnits")]
public class MilitaryUnit
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(100)]
    public string UnitType { get; set; } = string.Empty;

    public int Quantity { get; set; } = 0;
    public int InTraining { get; set; } = 0;
    public DateTime? TrainingCompletesAt { get; set; }

    public Kingdom Kingdom { get; set; } = null!;
    public UnitDefinition Definition { get; set; } = null!;
}
