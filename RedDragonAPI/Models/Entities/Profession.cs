using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Professions")]
public class Profession
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(50)]
    public string ProfessionType { get; set; } = string.Empty;

    public int WorkerCount { get; set; } = 0;
    public long ProductionPerTurn { get; set; } = 0;

    public Kingdom Kingdom { get; set; } = null!;
}
