using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("ThiefActionDefinitions")]
public class ThiefActionDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ThievesRequired { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal SuccessBaseRate { get; set; }

    [MaxLength(50)]
    public string? EffectType { get; set; }
}
