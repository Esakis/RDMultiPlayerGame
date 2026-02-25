using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Kingdoms")]
public class Kingdom
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Race { get; set; } = "Human";

    // Zasoby
    public int Land { get; set; } = 100;
    public long Gold { get; set; } = 50000;
    public long Food { get; set; } = 10000;
    public long Wood { get; set; } = 5000;
    public long Stone { get; set; } = 2000;
    public long Iron { get; set; } = 1000;
    public long Mana { get; set; } = 500;

    // Ludność
    public int Population { get; set; } = 1000;
    public int PopulationGrowthRate { get; set; } = 50;

    // Tury
    public int TurnsAvailable { get; set; } = 15;
    public int TurnsPerDay { get; set; } = 15;
    public int MaxTurns { get; set; } = 49;

    // Wiek księstwa
    public int Age { get; set; } = 0;

    // Koalicja
    public int? CoalitionId { get; set; }

    [MaxLength(50)]
    public string? CoalitionRole { get; set; }

    // Era
    public int EraId { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coalition? Coalition { get; set; }
    public Era Era { get; set; } = null!;
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<MilitaryUnit> MilitaryUnits { get; set; } = new List<MilitaryUnit>();
    public ICollection<Profession> Professions { get; set; } = new List<Profession>();
    public ICollection<Research> Researches { get; set; } = new List<Research>();
    public ICollection<ActiveSpell> ActiveSpells { get; set; } = new List<ActiveSpell>();
}
