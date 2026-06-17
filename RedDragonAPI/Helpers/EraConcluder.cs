using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Zakończenie ery po zbudowaniu Pałacu Sądu Ostatecznego (docs/MECHANIKA.md §12, §14.3):
/// zwycięska koalicja trafia do Panteonu, stara era zostaje zamknięta, a wszystkie księstwa
/// startują od nowa (100 akrów) w nowej erze.
/// </summary>
public static class EraConcluder
{
    private static readonly string[] StartingProfessions =
    {
        "Bezrobotni", "Alchemicy", "Chłopi", "Druidzi",
        "Kamieniarze", "Murarze", "Płatnerze", "Kupcy", "Magowie", "Naukowcy"
    };

    public static async Task ConcludeAsync(ApplicationDbContext ctx, Coalition winner)
    {
        var oldEra = await ctx.Eras.FirstOrDefaultAsync(e => e.Id == winner.EraId);
        if (oldEra == null || !oldEra.IsActive) return;

        // 1. Zamknij erę i zapisz zwycięzcę w Panteonie
        oldEra.IsActive = false;
        oldEra.EndedAt = DateTime.UtcNow;
        oldEra.WinningCoalitionId = winner.Id;
        ctx.Pantheons.Add(new Pantheon
        {
            EraId = oldEra.Id,
            CoalitionId = winner.Id,
            VictoryDate = DateTime.UtcNow
        });

        // 2. Nowa era
        int eraCount = await ctx.Eras.CountAsync();
        var newEra = new Era
        {
            Name = $"Era {eraCount + 1}",
            Theme = "Nowy wiek po zbudowaniu Pałacu Sądu Ostatecznego",
            IsActive = true,
            StartedAt = DateTime.UtcNow
        };
        ctx.Eras.Add(newEra);
        await ctx.SaveChangesAsync();

        // 3. Wyczyść stan księstw starej ery
        var kingdoms = await ctx.Kingdoms.Where(k => k.EraId == oldEra.Id).ToListAsync();
        var ids = kingdoms.Select(k => k.Id).ToList();

        ctx.QueuedActions.RemoveRange(ctx.QueuedActions.Where(q => ids.Contains(q.KingdomId)
            || (q.TargetKingdomId != null && ids.Contains(q.TargetKingdomId.Value))));
        ctx.MarketOrders.RemoveRange(ctx.MarketOrders.Where(o => ids.Contains(o.KingdomId)));
        ctx.Generals.RemoveRange(ctx.Generals.Where(g => ids.Contains(g.KingdomId)));
        ctx.Pacts.RemoveRange(ctx.Pacts.Where(p => ids.Contains(p.ProposerKingdomId) || ids.Contains(p.TargetKingdomId)));
        ctx.ActiveSpells.RemoveRange(ctx.ActiveSpells.Where(s => ids.Contains(s.KingdomId)));
        ctx.Buildings.RemoveRange(ctx.Buildings.Where(b => ids.Contains(b.KingdomId)));
        ctx.MilitaryUnits.RemoveRange(ctx.MilitaryUnits.Where(m => ids.Contains(m.KingdomId)));
        ctx.Researches.RemoveRange(ctx.Researches.Where(r => ids.Contains(r.KingdomId)));
        ctx.Professions.RemoveRange(ctx.Professions.Where(p => ids.Contains(p.KingdomId)));
        await ctx.SaveChangesAsync();

        // 4. Reset księstw i przeniesienie do nowej ery
        var races = await ctx.RaceDefinitions.AsNoTracking().ToListAsync();
        foreach (var k in kingdoms)
        {
            int tpd = races.FirstOrDefault(r => r.Name == k.Race)?.TurnsPerDay ?? 15;

            k.EraId = newEra.Id;
            k.Land = 100;
            k.Gold = 50000; k.Food = 10000; k.Stone = 2000;
            k.Budulec = 0; k.BudulecStored = 0; k.Weapons = 0; k.Mana = 0;
            k.Population = 1000; k.Popularity = 100; k.Wages = 50; k.Education = 0;
            k.SciencePoints = 0; k.CurrentResearchTech = null; k.LabyrinthActionsUsed = 0;
            k.TurnsPerDay = tpd; k.TurnsAvailable = tpd; k.TurnsCapacity = tpd; k.MaxTurns = tpd * 3 + 4;
            k.TurnNumber = 0; k.Age = 0;
            k.CurrentSpecialBuilding = null; k.SpecialBuildingProgress = 0; k.SpecialBuildingCost = 0;
            k.CoalitionId = null; k.CoalitionRole = null;
            k.IsProtected = true; k.ProtectionDaysLeft = 5;

            foreach (var profType in StartingProfessions)
                ctx.Professions.Add(new Profession
                {
                    KingdomId = k.Id,
                    ProfessionType = profType,
                    WorkerCount = profType == "Bezrobotni" ? 1000 : 0
                });
        }

        await ctx.SaveChangesAsync();
    }
}
