using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Tablica ogłoszeń koalicji (docs/TODO.md A5): wpisy w HTML, edytowane wyłącznie
/// przez Imperatora i Głównodowodzącego, widoczne dla całej koalicji.
/// Ogłoszenia nie znikają same — tylko ręczne usunięcie.
/// </summary>
[Table("CoalitionAnnouncements")]
public class CoalitionAnnouncement
{
    [Key]
    public int Id { get; set; }

    public int CoalitionId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Treść w HTML (formatowanie dowolne, renderowana u klienta z sanityzacją).</summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>Nazwa księstwa autora (bez FK — wpis przeżywa zmiany składu koalicji).</summary>
    [MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Coalition Coalition { get; set; } = null!;
}
