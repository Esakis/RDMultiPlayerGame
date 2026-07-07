namespace RedDragonAPI.Models.DTOs;

public class MarketOrderDto
{
    public int Id { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public long RemainingQuantity { get; set; }
    public long PricePerUnit { get; set; }
    public long TotalPrice { get; set; }
    public int KingdomId { get; set; }
    public string KingdomName { get; set; } = string.Empty;
    public bool IsOwn { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MarketViewDto
{
    public bool HasAccess { get; set; }
    public string? NoAccessReason { get; set; }
    public List<MarketOrderDto> Orders { get; set; } = new();
    public List<MarketOrderDto> MyOrders { get; set; } = new();
}

public class CreateMarketOrderDto
{
    public string OrderType { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public long PricePerUnit { get; set; }
}

public class FillMarketOrderDto
{
    public int OrderId { get; set; }
    public long Quantity { get; set; }
}

/// <summary>Kurs targu państwowego (stałe ceny wymiany złota za zasób).</summary>
public class ExchangeRateDto
{
    public string Resource { get; set; } = string.Empty;
    /// <summary>Cena kupna 1 jednostki (gracz płaci złotem).</summary>
    public long BuyPrice { get; set; }
    /// <summary>Cena skupu 1 jednostki (gracz otrzymuje złoto).</summary>
    public long SellPrice { get; set; }
}

public class ExchangeDto
{
    public string Resource { get; set; } = string.Empty;
    /// <summary>"Buy" = kup zasób za złoto, "Sell" = sprzedaj zasób za złoto.</summary>
    public string Direction { get; set; } = string.Empty;
    public long Quantity { get; set; }
}

public class MarketTransactionDto
{
    public int Id { get; set; }
    public string Resource { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public long PricePerUnit { get; set; }
    public long GrossGold { get; set; }
    public long Tax { get; set; }
    public bool IAmBuyer { get; set; }
    public string CounterpartyName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
