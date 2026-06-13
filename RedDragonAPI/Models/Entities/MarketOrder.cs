using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Oferta na rynku surowców (docs/MECHANIKA.md §13 „Rynek").
/// Handel surowcami między graczami — wymaga budynku Skrzyżowanie szlaków handlowych.
/// Model escrow: przy wystawieniu oferty zasób (Sell) lub złoto (Buy) jest deponowane
/// i trzymane „na rynku", a przy realizacji/anulowaniu przekazywane lub zwracane.
/// Złoto jest walutą — handluje się jedynie zasobami: jedzenie, kamień, broń, mana.
/// </summary>
[Table("MarketOrders")]
public class MarketOrder
{
    [Key]
    public int Id { get; set; }

    /// <summary>Księstwo wystawiające ofertę (sprzedawca dla Sell, kupujący dla Buy).</summary>
    public int KingdomId { get; set; }

    /// <summary>Sell (oferta sprzedaży zasobu) | Buy (zlecenie kupna zasobu).</summary>
    [Required, MaxLength(10)]
    public string OrderType { get; set; } = string.Empty;

    /// <summary>Food | Stone | Weapons | Mana</summary>
    [Required, MaxLength(20)]
    public string Resource { get; set; } = string.Empty;

    /// <summary>Wystawiona ilość zasobu (pierwotna).</summary>
    public long Quantity { get; set; }

    /// <summary>Ilość jeszcze niezrealizowana (zdeponowana w ofercie).</summary>
    public long RemainingQuantity { get; set; }

    /// <summary>Cena za jednostkę w złocie.</summary>
    public long PricePerUnit { get; set; }

    /// <summary>Open | Completed | Cancelled</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Kingdom Kingdom { get; set; } = null!;
}
