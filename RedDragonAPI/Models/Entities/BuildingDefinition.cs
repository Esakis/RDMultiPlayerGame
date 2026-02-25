using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("BuildingDefinitions")]
public class BuildingDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string BuildingType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Koszty budowy
    public int CostGold { get; set; } = 0;
    public int CostWood { get; set; } = 0;
    public int CostStone { get; set; } = 0;
    public int CostIron { get; set; } = 0;
    public int CostMana { get; set; } = 0;
    public int CostLand { get; set; } = 1;

    // Czas budowy
    public int BuildTime { get; set; } = 1;

    // Wymagania
    [MaxLength(100)]
    public string? RequiredBuildingType { get; set; }

    [MaxLength(100)]
    public string? RequiredTechnology { get; set; }

    // Efekty
    public bool IsSpecial { get; set; } = false;
    public int BonusTurnsPerDay { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal ProductionBonus { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal DefenseBonus { get; set; } = 0;

    public int PopulationCapacity { get; set; } = 0;
}
