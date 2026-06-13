using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;

namespace RedDragonAPI.Helpers;

/// <summary>Pomocnik stanu wojny między koalicjami (docs/MECHANIKA.md §12).</summary>
public static class WarHelper
{
    /// <summary>Czy dwie koalicje są w aktywnym stanie wojny (dowolny kierunek wpisu).</summary>
    public static async Task<bool> AreAtWarAsync(ApplicationDbContext ctx, int? coalitionA, int? coalitionB)
    {
        if (coalitionA == null || coalitionB == null) return false;
        return await ctx.Wars.AnyAsync(w => w.Status == "Active" &&
            ((w.DeclaringCoalitionId == coalitionA && w.TargetCoalitionId == coalitionB) ||
             (w.DeclaringCoalitionId == coalitionB && w.TargetCoalitionId == coalitionA)));
    }
}
