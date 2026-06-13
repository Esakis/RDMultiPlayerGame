using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Odczyt efektów ukończonych badań księstwa (docs/MECHANIKA.md §13).
/// Dla łańcuchów poziomowych wartość najwyższego ukończonego poziomu to pełny efekt,
/// więc rabaty/bonusy procentowe bierzemy jako MAX. Efekty addytywne (np. +tura)
/// sumujemy.
/// </summary>
public static class ResearchEffects
{
    /// <summary>Najwyższa wartość efektu danego typu wśród ukończonych badań (0, gdy brak).</summary>
    public static async Task<decimal> MaxEffectAsync(ApplicationDbContext ctx, int kingdomId, string effectType)
    {
        return await ctx.Researches
            .Where(r => r.KingdomId == kingdomId && r.IsCompleted && r.Tech.EffectType == effectType)
            .Select(r => r.Tech.EffectValue)
            .DefaultIfEmpty(0m)
            .MaxAsync();
    }

    /// <summary>Suma wartości efektów danego typu wśród ukończonych badań (efekty addytywne).</summary>
    public static async Task<decimal> SumEffectAsync(ApplicationDbContext ctx, int kingdomId, string effectType)
    {
        return await ctx.Researches
            .Where(r => r.KingdomId == kingdomId && r.IsCompleted && r.Tech.EffectType == effectType)
            .SumAsync(r => r.Tech.EffectValue);
    }
}
