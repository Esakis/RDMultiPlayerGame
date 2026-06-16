using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Generał wg oryginalnego Red Dragon (docs/MECHANIKA.md §11, docs/zrodla/urza-generalove.txt).
/// Limit 6 (8 z Pałacem; Człowiek zawsze 8). Poziom z doświadczenia:
/// lvl ≈ int((exp/100)^0,25) + 1.
/// </summary>
[Table("Generals")]
public class General
{
    [Key]
    public int Id { get; set; }

    public int KingdomId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Cecha główna: Wodz, Obronca, Mag, Zlodziej, Kupiec, Profesor</summary>
    [Required, MaxLength(50)]
    public string PrimaryTrait { get; set; } = string.Empty;

    /// <summary>Cecha drugorzędna (np. Uzdrawianie, Sabotaz, MagiaCzasu, BialaMagia…)</summary>
    [Required, MaxLength(50)]
    public string SecondaryTrait { get; set; } = string.Empty;

    public long Experience { get; set; } = 0;

    /// <summary>Czy prowadzi atak (poza księstwem do najbliższego przeliczenia)</summary>
    public bool IsOutside { get; set; } = false;

    /// <summary>Czy uwięziony przez wroga</summary>
    public bool IsImprisoned { get; set; } = false;

    /// <summary>Ranny do tej daty (nie bierze udziału w walkach)</summary>
    public DateTime? WoundedUntil { get; set; }

    /// <summary>
    /// Generał czeka w „poczekalni" na decyzję gracza (przyjąć / odrzucić).
    /// Oczekujący nie liczy się do siły księstwa ani limitu aktywnych generałów.
    /// </summary>
    public bool IsPending { get; set; } = false;

    /// <summary>
    /// Ile razy gracz przelosował cechę drugorzędną podczas zatrudniania (limit 2).
    /// </summary>
    public int SecondaryRerollsUsed { get; set; } = 0;

    public DateTime ArrivedAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;

    [NotMapped]
    public int Level => (int)Math.Pow(Math.Max(Experience, 0) / 100.0, 0.25) + 1;

    /// <summary>Doświadczenie potrzebne do osiągnięcia kolejnego poziomu (próg = 100 · poziom^4).</summary>
    [NotMapped]
    public long NextLevelExperience => 100L * (long)Math.Pow(Level, 4);

    /// <summary>Ile doświadczenia brakuje do awansu na kolejny poziom.</summary>
    [NotMapped]
    public long ExperienceToNextLevel => Math.Max(0, NextLevelExperience - Experience);
}
