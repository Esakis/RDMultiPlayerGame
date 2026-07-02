using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IDragonService
{
    Task<DragonStatusDto?> GetStatusAsync(int userId);

    /// <summary>
    /// Pasywne przychodzenie smoków co turę (model hybrydowy): bazowo niewielka szansa dla
    /// każdego księstwa, budynki smocze (Smokodrap/Portal/Ministerstwo) i badania o smokach
    /// mocno zwiększają liczbę. Dużo smoków na początku, szansa maleje liniowo do 0 przy 200.
    /// Zakłada załadowane kingdom.Buildings. Zwraca liczbę przybyłych smoków (zapisuje zmiany).
    /// </summary>
    Task<int> ProcessTurnArrivalAsync(Kingdom kingdom);
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
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);
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

        // Orientacyjny koszt przywołania (baza 500 many · skala ziemi · mnożnik od smoków)
        long summonCost = (long)(500m * (1m + kingdom.Land / 2000m)
                                 * DragonHelper.SummonCostMultiplier(dragons));

        // Oczekiwane pasywne przyjścia na turę — ta sama krzywa co ProcessTurnArrivalAsync
        double taper = Math.Max(0.0, 1.0 - (double)dragons / PassiveArrivalCeiling);
        double lure = 2.0;
        if (DragonHelper.Has(kingdom, "Smokodrap")) lure += 2.0;
        if (DragonHelper.Has(kingdom, "Portal")) lure += 3.0;
        if (DragonHelper.Has(kingdom, "MinisterstwoSmokow")) lure += 4.0;
        lure += draco;
        decimal expectedArrivals = dragons >= Math.Min(cap, PassiveArrivalCeiling)
            ? 0m
            : Math.Round((decimal)(lure * taper), 1);

        return new DragonStatusDto
        {
            Dragons = dragons,
            Cap = cap,
            CapSource = capSource,
            DracoLevel = draco,
            DracoBonusPct = dracoBonus * 100m,
            HasPortal = DragonHelper.Has(kingdom, "Portal"),
            HasSmokodrap = DragonHelper.Has(kingdom, "Smokodrap"),
            HasMinisterstwo = DragonHelper.Has(kingdom, "MinisterstwoSmokow"),
            CanSummon = canSummon,
            CannotSummonReason = reason,
            PowerMultiplier = Math.Round(powerMult, 3),
            FlatAttackBonus = dragons * 100,
            SummonCostEstimate = summonCost,
            Mana = kingdom.Mana,
            ExpectedArrivalsPerTurn = expectedArrivals
        };
    }

    /// <summary>Próg, do którego smoki przychodzą pasywnie co turę (krzywa maleje do 0 przy 200).</summary>
    private const int PassiveArrivalCeiling = 200;

    public async Task<int> ProcessTurnArrivalAsync(Kingdom kingdom)
    {
        var dragonDef = await _context.UnitDefinitions
            .FirstOrDefaultAsync(u => u.Race == kingdom.Race && u.UnitType.EndsWith("_Smok"));
        if (dragonDef == null) return 0; // rasa nie ma smoków

        long dragons = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Smok"))
            .SumAsync(m => (long)m.Quantity);

        int draco = await _context.Researches
            .CountAsync(r => r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Smoko"));
        long cap = DragonHelper.ComputeCap(kingdom, draco);

        // Pasywne wabienie zatrzymuje się przy 200 (wymóg) i nigdy nie przekracza limitu budynków.
        long ceiling = Math.Min(cap, PassiveArrivalCeiling);
        if (dragons >= ceiling) return 0;

        // Współczynnik malejący: 1 przy 0 smokach, 0 przy 200.
        double taper = 1.0 - (double)dragons / PassiveArrivalCeiling;

        // „Siła wabienia” (Hybryda): baza dla wszystkich + duży dodatek z budynków i badań.
        double lure = 2.0; // baza — nawet bez budynku kilka smoków na starcie
        if (DragonHelper.Has(kingdom, "Smokodrap")) lure += 2.0;
        if (DragonHelper.Has(kingdom, "Portal")) lure += 3.0;
        if (DragonHelper.Has(kingdom, "MinisterstwoSmokow")) lure += 4.0;
        lure += draco; // każdy poziom badań o smokach +1

        // Oczekiwana liczba smoków tej tury = siła wabienia × współczynnik malejący.
        double expected = lure * taper;
        int arrivals = (int)expected;
        if (Random.Shared.NextDouble() < expected - arrivals) arrivals++;
        arrivals = (int)Math.Min(arrivals, ceiling - dragons);
        if (arrivals <= 0) return 0;

        var unit = await _context.MilitaryUnits
            .FirstOrDefaultAsync(m => m.KingdomId == kingdom.Id && m.UnitType == dragonDef.UnitType);
        if (unit == null)
            _context.MilitaryUnits.Add(new MilitaryUnit
            {
                KingdomId = kingdom.Id, UnitType = dragonDef.UnitType, Quantity = arrivals
            });
        else
            unit.Quantity += arrivals;

        await _context.SaveChangesAsync();
        return arrivals;
    }
}
