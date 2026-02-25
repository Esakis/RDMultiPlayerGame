using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Pantheon")]
public class Pantheon
{
    [Key]
    public int Id { get; set; }

    public int EraId { get; set; }
    public int CoalitionId { get; set; }
    public DateTime VictoryDate { get; set; }

    public Era Era { get; set; } = null!;
    public Coalition Coalition { get; set; } = null!;
}
