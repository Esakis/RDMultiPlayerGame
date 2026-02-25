using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Eras")]
public class Era
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Theme { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    public int? WinningCoalitionId { get; set; }

    public bool IsActive { get; set; } = true;

    public Coalition? WinningCoalition { get; set; }
    public ICollection<Coalition> Coalitions { get; set; } = new List<Coalition>();
    public ICollection<Kingdom> Kingdoms { get; set; } = new List<Kingdom>();
}
