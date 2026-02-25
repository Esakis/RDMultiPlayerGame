using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("UnitDefinitions")]
public class UnitDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string UnitType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Race { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Koszty
    public int CostGold { get; set; } = 0;
    public int CostIron { get; set; } = 0;
    public int CostWood { get; set; } = 0;
    public int CostFood { get; set; } = 0;

    // Statystyki
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int Upkeep { get; set; } = 0;

    // Wymagania
    [Required, MaxLength(100)]
    public string RequiredBuilding { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RequiredTech { get; set; }

    public int TrainingTime { get; set; } = 1;
}
