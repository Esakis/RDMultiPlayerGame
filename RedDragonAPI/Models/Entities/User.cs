using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    /// <summary>Rola konta: Player | Admin (super admin ustawia m.in. opłatę za księstwo).</summary>
    [MaxLength(20)]
    public string Role { get; set; } = "Player";

    /// <summary>
    /// Aktualnie wybrane księstwo (konto może mieć ich wiele) — wszystkie akcje w grze
    /// wykonywane są w kontekście tego księstwa. Null = nic nie wybrano (np. konto admina).
    /// </summary>
    public int? ActiveKingdomId { get; set; }

    public ICollection<Kingdom> Kingdoms { get; set; } = new List<Kingdom>();
}
