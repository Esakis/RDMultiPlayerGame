using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Buildings")]
public class Building
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(100)]
    public string BuildingType { get; set; } = string.Empty;

    public int Quantity { get; set; } = 0;
    public int Level { get; set; } = 1;
    public bool IsUnderConstruction { get; set; } = false;
    public DateTime? ConstructionCompletesAt { get; set; }

    public Kingdom Kingdom { get; set; } = null!;
    public BuildingDefinition Definition { get; set; } = null!;
}
