using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("TechnologyDefinitions")]
public class TechnologyDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TechType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Level { get; set; } = 1;

    // Koszty
    public int CostGold { get; set; }
    public int ResearchTime { get; set; }

    // Wymagania
    [MaxLength(100)]
    public string? RequiredTech { get; set; }

    [MaxLength(100)]
    public string? RequiredBuilding { get; set; }

    // Efekty
    [MaxLength(50)]
    public string? EffectType { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EffectValue { get; set; }
}
