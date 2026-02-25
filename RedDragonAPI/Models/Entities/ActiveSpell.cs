using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("ActiveSpells")]
public class ActiveSpell
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(100)]
    public string SpellType { get; set; } = string.Empty;

    public int Power { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CastAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;
    public SpellDefinition Spell { get; set; } = null!;
}
