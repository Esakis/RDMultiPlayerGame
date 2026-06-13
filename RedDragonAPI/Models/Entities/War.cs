using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Wojna między dwiema koalicjami (docs/MECHANIKA.md §12, §14.4). Stan wojny pozwala
/// atakować członków wrogiej koalicji i rzucać na nich zaklęcia. Wojna jest dwukierunkowa
/// (wystarczy jeden wpis dla pary koalicji). Wypowiedzenie do 20:00.
/// </summary>
[Table("Wars")]
public class War
{
    [Key]
    public int Id { get; set; }

    public int EraId { get; set; }

    public int DeclaringCoalitionId { get; set; }
    public int TargetCoalitionId { get; set; }

    /// <summary>Active | Ended</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime DeclaredAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public Era Era { get; set; } = null!;
    public Coalition DeclaringCoalition { get; set; } = null!;
    public Coalition TargetCoalition { get; set; } = null!;
}
