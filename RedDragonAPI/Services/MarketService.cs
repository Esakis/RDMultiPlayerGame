using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IMarketService
{
    Task<MarketViewDto> GetMarketAsync(int userId);
    Task<ServiceResult> CreateOrderAsync(int userId, CreateMarketOrderDto dto);
    Task<ServiceResult> FillOrderAsync(int userId, FillMarketOrderDto dto);
    Task<ServiceResult> CancelOrderAsync(int userId, int orderId);
}

/// <summary>
/// Rynek surowców (docs/MECHANIKA.md §13). Handel surowcami między graczami
/// z wykorzystaniem księgi zleceń i depozytu (escrow):
/// - oferta sprzedaży (Sell) deponuje zasób; przy realizacji kupujący płaci złoto,
///   a sprzedawca dostaje złoto i kupujący zdeponowany zasób,
/// - zlecenie kupna (Buy) deponuje złoto; przy realizacji sprzedawca dostarcza zasób
///   i otrzymuje zdeponowane złoto, a wystawca zlecenia — zasób.
/// Dostęp do rynku wymaga budynku Skrzyżowanie szlaków handlowych.
/// </summary>
public class MarketService : IMarketService
{
    public const string MarketBuildingType = "SkrzyzowanieSzlakow";

    /// <summary>Zasoby dozwolone w handlu (złoto jest walutą, nie towarem).</summary>
    public static readonly string[] TradableResources = { "Food", "Stone", "Weapons", "Mana" };

    public static readonly string[] OrderTypes = { "Sell", "Buy" };

    private readonly ApplicationDbContext _context;

    public MarketService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Kingdom?> GetKingdomAsync(int userId) =>
        await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

    private static bool HasMarketAccess(Kingdom kingdom) =>
        kingdom.Buildings.Any(b => b.BuildingType == MarketBuildingType
            && b.Quantity > 0 && !b.IsUnderConstruction);

    private static long GetResource(Kingdom k, string resource) => resource switch
    {
        "Food" => k.Food,
        "Stone" => k.Stone,
        "Weapons" => k.Weapons,
        "Mana" => k.Mana,
        _ => 0
    };

    private static void AddResource(Kingdom k, string resource, long amount)
    {
        switch (resource)
        {
            case "Food": k.Food += amount; break;
            case "Stone": k.Stone += amount; break;
            case "Weapons": k.Weapons += amount; break;
            case "Mana": k.Mana += amount; break;
        }
    }

    public async Task<MarketViewDto> GetMarketAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return new MarketViewDto { HasAccess = false, NoAccessReason = "Nie znaleziono księstwa." };

        var orders = await _context.MarketOrders
            .Where(o => o.Status == "Open")
            .Include(o => o.Kingdom)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        MarketOrderDto Map(MarketOrder o) => new()
        {
            Id = o.Id,
            OrderType = o.OrderType,
            Resource = o.Resource,
            Quantity = o.Quantity,
            RemainingQuantity = o.RemainingQuantity,
            PricePerUnit = o.PricePerUnit,
            TotalPrice = o.RemainingQuantity * o.PricePerUnit,
            KingdomId = o.KingdomId,
            KingdomName = o.Kingdom.Name,
            IsOwn = o.KingdomId == kingdom.Id,
            CreatedAt = o.CreatedAt
        };

        return new MarketViewDto
        {
            HasAccess = HasMarketAccess(kingdom),
            NoAccessReason = HasMarketAccess(kingdom)
                ? null
                : "Aby handlować na rynku, musisz wybudować Skrzyżowanie szlaków handlowych.",
            Orders = orders.Where(o => o.KingdomId != kingdom.Id).Select(Map).ToList(),
            MyOrders = orders.Where(o => o.KingdomId == kingdom.Id).Select(Map).ToList()
        };
    }

    public async Task<ServiceResult> CreateOrderAsync(int userId, CreateMarketOrderDto dto)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (!HasMarketAccess(kingdom))
            return ServiceResult.Fail("Wymagane Skrzyżowanie szlaków handlowych.");
        if (!OrderTypes.Contains(dto.OrderType))
            return ServiceResult.Fail("Nieznany typ oferty.");
        if (!TradableResources.Contains(dto.Resource))
            return ServiceResult.Fail("Tym zasobem nie można handlować.");
        if (dto.Quantity <= 0)
            return ServiceResult.Fail("Ilość musi być dodatnia.");
        if (dto.PricePerUnit <= 0)
            return ServiceResult.Fail("Cena za jednostkę musi być dodatnia.");

        if (dto.OrderType == "Sell")
        {
            // Deponujemy zasób ze skarbca sprzedawcy
            if (GetResource(kingdom, dto.Resource) < dto.Quantity)
                return ServiceResult.Fail("Za mało zasobu na wystawienie oferty.");
            AddResource(kingdom, dto.Resource, -dto.Quantity);
        }
        else // Buy
        {
            // Deponujemy złoto kupującego
            long cost = dto.Quantity * dto.PricePerUnit;
            if (kingdom.Gold < cost)
                return ServiceResult.Fail("Za mało złota na wystawienie zlecenia kupna.");
            kingdom.Gold -= cost;
        }

        _context.MarketOrders.Add(new MarketOrder
        {
            KingdomId = kingdom.Id,
            OrderType = dto.OrderType,
            Resource = dto.Resource,
            Quantity = dto.Quantity,
            RemainingQuantity = dto.Quantity,
            PricePerUnit = dto.PricePerUnit,
            Status = "Open"
        });
        await _context.SaveChangesAsync();

        return ServiceResult.Ok(dto.OrderType == "Sell"
            ? "Wystawiono ofertę sprzedaży na rynku."
            : "Wystawiono zlecenie kupna na rynku.");
    }

    public async Task<ServiceResult> FillOrderAsync(int userId, FillMarketOrderDto dto)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");
        if (!HasMarketAccess(kingdom))
            return ServiceResult.Fail("Wymagane Skrzyżowanie szlaków handlowych.");

        var order = await _context.MarketOrders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.Status == "Open");
        if (order == null)
            return ServiceResult.Fail("Oferta już nie istnieje.");
        if (order.KingdomId == kingdom.Id)
            return ServiceResult.Fail("Nie możesz realizować własnej oferty.");
        if (dto.Quantity <= 0)
            return ServiceResult.Fail("Ilość musi być dodatnia.");
        if (dto.Quantity > order.RemainingQuantity)
            return ServiceResult.Fail("Oferta nie ma tylu jednostek.");

        var owner = await _context.Kingdoms.FirstOrDefaultAsync(k => k.Id == order.KingdomId);
        if (owner == null)
            return ServiceResult.Fail("Nie znaleziono wystawcy oferty.");

        long units = dto.Quantity;
        long value = units * order.PricePerUnit;

        if (order.OrderType == "Sell")
        {
            // Wystawca sprzedaje zasób — realizujący KUPUJE: płaci złoto, dostaje zasób z depozytu
            if (kingdom.Gold < value)
                return ServiceResult.Fail("Za mało złota na zakup.");
            kingdom.Gold -= value;
            AddResource(kingdom, order.Resource, units);
            owner.Gold += value;
        }
        else // Buy
        {
            // Wystawca chce kupić zasób — realizujący SPRZEDAJE: dostarcza zasób, dostaje złoto z depozytu
            if (GetResource(kingdom, order.Resource) < units)
                return ServiceResult.Fail("Za mało zasobu na sprzedaż.");
            AddResource(kingdom, order.Resource, -units);
            kingdom.Gold += value;
            AddResource(owner, order.Resource, units);
        }

        order.RemainingQuantity -= units;
        if (order.RemainingQuantity <= 0)
            order.Status = "Completed";

        await _context.SaveChangesAsync();
        return ServiceResult.Ok(order.OrderType == "Sell"
            ? $"Kupiono {units} {ResourceName(order.Resource)} za {value} złota."
            : $"Sprzedano {units} {ResourceName(order.Resource)} za {value} złota.");
    }

    public async Task<ServiceResult> CancelOrderAsync(int userId, int orderId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult.Fail("Nie znaleziono księstwa.");

        var order = await _context.MarketOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.KingdomId == kingdom.Id && o.Status == "Open");
        if (order == null)
            return ServiceResult.Fail("Nie znaleziono Twojej aktywnej oferty.");

        // Zwrot depozytu reszty oferty
        if (order.OrderType == "Sell")
            AddResource(kingdom, order.Resource, order.RemainingQuantity);
        else
            kingdom.Gold += order.RemainingQuantity * order.PricePerUnit;

        order.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Ofertę wycofano, depozyt zwrócono.");
    }

    private static string ResourceName(string resource) => resource switch
    {
        "Food" => "jedzenia",
        "Stone" => "kamienia",
        "Weapons" => "broni",
        "Mana" => "many",
        _ => resource
    };
}
