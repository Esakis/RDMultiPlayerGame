using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Research")]
public class Research
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(100)]
    public string TechType { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;
    public bool IsInProgress { get; set; } = false;
    public DateTime? CompletesAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Kingdom Kingdom { get; set; } = null!;
    public TechnologyDefinition Tech { get; set; } = null!;
}
