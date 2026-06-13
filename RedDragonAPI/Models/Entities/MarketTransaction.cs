using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Zrealizowana transakcja na rynku (docs/MECHANIKA.md §13). Kupujący otrzymuje zasób,
/// sprzedający — złoto pomniejszone o podatek rynkowy.
/// </summary>
[Table("MarketTransactions")]
public class MarketTransaction
{
    [Key]
    public int Id { get; set; }

    /// <summary>Księstwo, które otrzymało zasób.</summary>
    public int BuyerKingdomId { get; set; }

    /// <summary>Księstwo, które otrzymało złoto.</summary>
    public int SellerKingdomId { get; set; }

    [Required, MaxLength(20)]
    public string Resource { get; set; } = string.Empty;

    public long Quantity { get; set; }
    public long PricePerUnit { get; set; }
    public long GrossGold { get; set; }
    public long Tax { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Kingdom BuyerKingdom { get; set; } = null!;
    public Kingdom SellerKingdom { get; set; } = null!;
}
