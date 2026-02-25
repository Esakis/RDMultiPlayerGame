using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Coalitions")]
public class Coalition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Tag { get; set; }

    public int? LeaderKingdomId { get; set; }

    public int EraId { get; set; }

    public int MaxMembers { get; set; } = 17;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PSOProgress { get; set; } = 0.00m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Era Era { get; set; } = null!;
    public Kingdom? Leader { get; set; }
    public ICollection<Kingdom> Members { get; set; } = new List<Kingdom>();
}
