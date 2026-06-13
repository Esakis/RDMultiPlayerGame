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
    public DateTime? CompletesAt { get; set; }       // legacy
    public DateTime? CompletedAt { get; set; }

    /// <summary>Zainwestowane Punkty Nauki (mechanika manuala, docs/MECHANIKA.md §13).</summary>
    public long InvestedScience { get; set; } = 0;

    public Kingdom Kingdom { get; set; } = null!;
    public TechnologyDefinition Tech { get; set; } = null!;
}
