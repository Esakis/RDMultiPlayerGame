using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Mechanika smoków (docs/MECHANIKA.md §8, §9). Smoki wzmacniają armię
/// (BattleCalculator: ×(1+r/(50+r)) + r·100). Limit smoków rośnie z budynkami
/// (Smokodrap → Portal → Ministerstwo smoków) i badaniami DragonKnowledge
/// (Smokoastronomia/anatomia/dynamika: +6/16/20%). Koszt przywołania rośnie
/// stromo z liczbą smoków wg wzoru manuala.
/// </summary>
public static class DragonHelper
{
    public const int BaseCap = 50;

    public static bool Has(Kingdom k, string buildingType) =>
        k.Buildings.Any(b => b.BuildingType == buildingType && b.Quantity > 0 && !b.IsUnderConstruction);

    /// <summary>Maksymalna liczba smoków: baza wg budynków × bonus z badań.</summary>
    public static long ComputeCap(Kingdom kingdom, int dracoLevel)
    {
        long baseCap = Has(kingdom, "MinisterstwoSmokow") ? 200
            : Has(kingdom, "Portal") ? 120
            : Has(kingdom, "Smokodrap") ? 80
            : BaseCap;

        decimal dracoBonus = dracoLevel switch
        {
            >= 3 => 0.20m,
            2 => 0.16m,
            1 => 0.06m,
            _ => 0m
        };

        return (long)(baseCap * (1m + dracoBonus));
    }

    /// <summary>
    /// Mnożnik kosztu przywołania wg liczby posiadanych smoków D:
    /// (D²·0,0001 + 0,2) · (max(50,D)/100)². Pierwsze smoki tanie, kolejne bardzo drogie.
    /// </summary>
    public static decimal SummonCostMultiplier(long dragons)
    {
        decimal a = (decimal)(dragons * dragons) * 0.0001m + 0.2m;
        double b = Math.Pow(Math.Max(50, dragons) / 100.0, 2);
        return a * (decimal)b;
    }
}
