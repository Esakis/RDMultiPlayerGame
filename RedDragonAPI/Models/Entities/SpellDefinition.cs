using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("SpellDefinitions")]
public class SpellDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string SpellType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ManaCost { get; set; }
    public int PowerLevel { get; set; }

    [MaxLength(50)]
    public string? EffectType { get; set; }

    [MaxLength(50)]
    public string? TargetType { get; set; }

    // Zaklęcie limitowane (niszczące) — standardowo max 2 przechodzą na cel za przeliczenie
    public bool IsLimited { get; set; } = false;

    // Zaklęcie rasowe (np. Metamagia — Dżin); null = dostępne wg ksiąg magii
    [MaxLength(50)]
    public string? RequiredRace { get; set; }

    // Minimalna liczba ksiąg magii potrzebna do rzucenia (0-5)
    public int RequiredBooks { get; set; } = 1;
}
