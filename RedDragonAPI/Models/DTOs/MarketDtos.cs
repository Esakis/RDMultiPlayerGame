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
