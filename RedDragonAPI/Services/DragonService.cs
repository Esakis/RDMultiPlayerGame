using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IDragonService
{
    Task<DragonStatusDto?> GetStatusAsync(int userId);
}

/// <summary>
/// Status smoków (docs/MECHANIKA.md §8, §9). Smoki zdobywa się zaklęciem
/// „Przywołanie Smoka" (Magia) oraz biernie przez Portal przy przeliczeniu.
/// Limit i bonusy patrz <see cref="DragonHelper"/>.
/// </summary>
public class DragonService : IDragonService
{
    private readonly ApplicationDbContext _context;

    public DragonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DragonStatusDto?> GetStatusAsync(int userId)
    {
        var kingdom = await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
        if (kingdom == null) return null;

        long dragons = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Smok"))
            .SumAsync(m => (long)m.Quantity);

        int draco = await _context.Researches
            .CountAsync(r => r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Smoko"));

        long cap = DragonHelper.ComputeCap(kingdom, draco);

        string capSource = DragonHelper.Has(kingdom, "MinisterstwoSmokow") ? "Ministerstwo smoków"
            : DragonHelper.Has(kingdom, "Portal") ? "Portal"
            : DragonHelper.Has(kingdom, "Smokodrap") ? "Smokodrap"
            : "Baza";

        decimal dracoBonus = draco switch { >= 3 => 0.20m, 2 => 0.16m, 1 => 0.06m, _ => 0m };

        var race = await _context.RaceDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == kingdom.Race);

        bool knowsSpell = (race?.MagicBooks ?? 0) >= 3; // „Przywołanie Smoka" — 3 księgi
        bool atCap = dragons >= cap;
        bool canSummon = knowsSpell && !atCap;
        string? reason = !knowsSpell
            ? "Twoja rasa nie zna magii przywoływania (potrzeba 3 ksiąg). Smoki możesz wabić Portalem."
            : atCap ? $"Osiągnięto limit smoków ({cap})."
            : null;

        decimal powerMult = 1m + (decimal)dragons / (50m + dragons);

        return new DragonStatusDto
        {
            Dragons = dragons,
            Cap = cap,
            CapSource = capSource,
            DracoLevel = draco,
            DracoBonusPct = dracoBonus * 100m,
            HasPortal = DragonHelper.Has(kingdom, "Portal"),
            CanSummon = canSummon,
            CannotSummonReason = reason,
            PowerMultiplier = Math.Round(powerMult, 3),
            FlatAttackBonus = dragons * 100
        };
    }
}
