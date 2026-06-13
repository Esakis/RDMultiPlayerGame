using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

/// <summary>
/// Przetwarzanie tury wg wzorów oryginalnego Red Dragon
/// (manual gry, strona „vzorce" — szczegóły: docs/MECHANIKA.md).
/// </summary>
public class ResourceService : IResourceService
{
    private readonly ApplicationDbContext _context;

    // Bazowa produkcja na pracownika za turę (wartości przybliżone — oryginalne
    // bazy nie są udokumentowane; relacje między profesjami zachowane).
    private const long AlchemistGoldBase = 10;
    private const long FarmerFoodBase = 5;
    private const long DruidManaBase = 1;
    private const long StonemasonStoneBase = 5;
    private const long ArmorerWeaponsBase = 3;
    private const decimal MasonStonePerBudulec = 2m;

    // Badania (docs/MECHANIKA.md §13): bazowe SP na naukowca/turę oraz limit SP/turę
    // wg poziomu dziedziny „Wynalazczość" (Rozwój): 20k / 35k / 50k / 100k / 130k / 150k.
    private const decimal ScientistSciencePerWorker = 25m;
    private static readonly long[] DevelopmentCaps = { 20_000, 35_000, 50_000, 100_000, 130_000, 150_000 };

    public ResourceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task GenerateResourcesForKingdomAsync(Kingdom kingdom)
    {
        if (kingdom.Buildings == null || !kingdom.Buildings.Any())
        {
            await _context.Entry(kingdom)
                .Collection(k => k.Buildings)
                .Query()
                .Include(b => b.Definition)
                .LoadAsync();
        }

        if (kingdom.Professions == null || !kingdom.Professions.Any())
        {
            await _context.Entry(kingdom)
                .Collection(k => k.Professions)
                .LoadAsync();
        }

        var race = await _context.RaceDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == kingdom.Race)
            ?? new RaceDefinition { Name = kingdom.Race };

        bool Has(string buildingType) => kingdom.Buildings
            .Any(b => b.BuildingType == buildingType && b.Quantity > 0 && !b.IsUnderConstruction);

        // === 1. Produkcja profesji ===
        // produktywność = (100 − pn·0,9)/100 · (1+rb) · (1+pv) · (1+cech) · ...
        // pn — % nowicjuszy (nowicjusz pracuje na 10%), rb — bonus rasowy,
        // pv — wynalezienie (tu: Education z naukowców).
        // Bonusy badań: ogólna produkcja (Wynalazczość), handel (Rachunkowość), kamień (Górnictwo)
        decimal productionBonus = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "ProductionBonus");
        decimal merchantResearchBonus = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "MerchantBonus");
        decimal stoneResearchBonus = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "StoneBonus");
        double scienceBonus = (double)await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "ScienceBonus");

        decimal inventedBonus = 1m + (kingdom.Education / 100m) + productionBonus;

        decimal Productivity(Profession prof, decimal raceBonus)
        {
            decimal novicePct = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100m
                : 0m;
            decimal baseEff = (100m - novicePct * 0.9m) / 100m;
            return baseEff * (1m + raceBonus) * inventedBonus;
        }

        long merchantGold = 0;
        foreach (var prof in kingdom.Professions)
        {
            long production = 0;

            switch (prof.ProfessionType)
            {
                case "Alchemicy":
                    production = (long)(prof.WorkerCount * AlchemistGoldBase * Productivity(prof, race.BonusAlchemists));
                    kingdom.Gold += production;
                    break;
                case "Chłopi":
                    production = (long)(prof.WorkerCount * FarmerFoodBase * Productivity(prof, race.BonusFarmers));
                    kingdom.Food += production;
                    break;
                case "Druidzi":
                    production = (long)(prof.WorkerCount * DruidManaBase * Productivity(prof, race.BonusDruids));
                    kingdom.Mana += production;
                    break;
                case "Kamieniarze":
                    production = (long)(prof.WorkerCount * StonemasonStoneBase * Productivity(prof, race.BonusStonemasons) * (1m + stoneResearchBonus));
                    kingdom.Stone += production;
                    break;
                case "Murarze":
                    // Murarze przerabiają kamień na budulec (infrapunkty)
                    long stoneNeeded = (long)(prof.WorkerCount * MasonStonePerBudulec);
                    long stoneUsed = Math.Min(stoneNeeded, kingdom.Stone);
                    production = (long)(stoneUsed / MasonStonePerBudulec * Productivity(prof, race.BonusMasons));
                    kingdom.Stone -= stoneUsed;
                    kingdom.Budulec += production;
                    break;
                case "Płatnerze":
                    production = (long)(prof.WorkerCount * ArmorerWeaponsBase * Productivity(prof, race.BonusArmorers));
                    kingdom.Weapons += production;
                    break;
                case "Kupcy":
                    // Oryginalny wzór: złoto na kupca = 500·z/(z + ob·10),
                    // z — obszar własny + obszar partnerów paktów handlowych
                    if (prof.WorkerCount > 0)
                    {
                        long tradeLand = kingdom.Land + await _context.Pacts
                            .Where(p => p.PactType == "Handlowy" && p.Status == "Active"
                                        && (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id))
                            .Select(p => p.ProposerKingdomId == kingdom.Id
                                ? p.TargetKingdom.Land
                                : p.ProposerKingdom.Land)
                            .SumAsync(l => (long)l);
                        decimal goldPerMerchant = 500m * tradeLand / (tradeLand + prof.WorkerCount * 10m);
                        production = (long)(prof.WorkerCount * goldPerMerchant * Productivity(prof, race.BonusMerchants) * (1m + merchantResearchBonus));
                        merchantGold = production;
                        kingdom.Gold += production;
                    }
                    break;
                case "Naukowcy":
                    // Naukowcy produkują Punkty Nauki (SP) inwestowane w wybraną dziedzinę.
                    // Limit SP/turę zależy od poziomu „Wynalazczości" i rasy (Człowiek +33%,
                    // Goblin −20%). Nadprodukcja daje szansę na „przełom" (kilkukrotny przyrost).
                    if (prof.WorkerCount > 0)
                    {
                        decimal spRaw = prof.WorkerCount * ScientistSciencePerWorker
                                        * Productivity(prof, race.BonusScientists);
                        long cap = await ScienceCapAsync(kingdom);
                        long sp = (long)Math.Min(spRaw, cap);

                        if (spRaw > cap && cap > 0)
                        {
                            // Empiryzm (ScienceBonus): +szansa i +wartość przełomu
                            double overChance = Math.Min(0.10 + scienceBonus, 0.01 + 0.09 * (double)((spRaw - cap) / cap) + scienceBonus);
                            if (Random.Shared.NextDouble() < overChance)
                                sp = (long)(sp * (1.5 + Random.Shared.NextDouble() * 2.5) * (1 + scienceBonus)); // przełom: 1,5–4×
                        }

                        production = sp;
                        await InvestScienceAsync(kingdom, sp);
                    }
                    break;
                case "Magowie":
                    // Magowie nie produkują surowców — ich liczba to siła magiczna księstwa
                    production = prof.WorkerCount;
                    break;
            }

            prof.ProductionPerTurn = production;
        }

        // === 2. Produkcja manufaktur ===
        // p = (z/(z + m·25)) · k — k: sad owocowy 400 (jedzenie), kamieniołom 40 (kamień),
        // kopalnia diamentów 4000 (złoto), manowe jeziorko 40 (mana).
        var manufactory = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "Manufaktura" && b.Quantity > 0 && !b.IsUnderConstruction);
        if (manufactory != null)
        {
            decimal m = manufactory.Quantity;
            decimal perManufactory = kingdom.Land / (kingdom.Land + m * 25m) * 400m;
            kingdom.Food += (long)(perManufactory * m);
        }

        // === 3. Pensje i żołd ===
        int totalWorkers = kingdom.Professions
            .Where(p => p.ProfessionType != "Bezrobotni")
            .Sum(p => p.WorkerCount);
        long wagesCost = (long)totalWorkers * kingdom.Wages;
        kingdom.Gold -= wagesCost;

        var militaryUnits = await _context.MilitaryUnits
            .Where(mu => mu.KingdomId == kingdom.Id)
            .Include(mu => mu.Definition)
            .ToListAsync();

        bool armyIsFree = kingdom.Race is "Nekromant"; // armia Nekromanty bez żołdu
        if (!armyIsFree)
        {
            long armyPay = 0;
            foreach (var unit in militaryUnits)
            {
                if (unit.Definition == null || unit.Quantity <= 0) continue;
                string type = unit.UnitType;
                if (type.EndsWith("_Hoplita"))
                {
                    // hoplita: żołd 0,2 × płaca
                    armyPay += (long)(unit.Quantity * kingdom.Wages * 0.2m);
                }
                else if (type.EndsWith("_Paladyn") || IsEliteTwo(unit))
                {
                    // E2: żołd = płaca · (atak + obrona) / 10
                    armyPay += (long)(unit.Quantity * kingdom.Wages *
                        (unit.Definition.AttackPower + unit.Definition.DefensePower) / 10m);
                }
                // E1, złodzieje, machiny i smoki nie pobierają żołdu
            }
            kingdom.Gold -= armyPay;
        }

        // === 4. Jedzenie ===
        // ludność je wg rasy (Olbrzym 2); armia je 1 (Nekromant i Wampir — armia nie je)
        bool armyEats = kingdom.Race is not ("Nekromant" or "Wampir");
        int totalSoldiers = militaryUnits
            .Where(u => !u.UnitType.EndsWith("_Smok"))
            .Sum(u => u.Quantity);
        long foodNeeded = (long)(kingdom.Population * race.FoodPerPop)
                          + (armyEats ? totalSoldiers : 0);
        kingdom.Food -= foodNeeded;

        if (kingdom.Food < 0)
        {
            kingdom.Food = 0;
            // Głód: popularność −1…−15 (wg skali niedoboru), ludzie umierają/uciekają
            kingdom.Popularity = Math.Max(0, kingdom.Popularity - 10);
            int starving = (int)(kingdom.Population * 0.05);
            kingdom.Population = Math.Max(100, kingdom.Population - starving);
            // armia umiera w głodzie (chyba że rasa zwolniona)
            if (armyEats)
            {
                foreach (var unit in militaryUnits.Where(u => !u.UnitType.EndsWith("_Smok") && u.Quantity > 0))
                    unit.Quantity = Math.Max(0, unit.Quantity - (int)(unit.Quantity * 0.05));
            }
        }

        // === 5. Budulec: najpierw budynek specjalny w budowie, reszta do magazynu ===
        if (!string.IsNullOrEmpty(kingdom.CurrentSpecialBuilding) && kingdom.Budulec > 0)
        {
            int needed = kingdom.SpecialBuildingCost - kingdom.SpecialBuildingProgress;
            int used = (int)Math.Min(kingdom.Budulec, needed);
            kingdom.SpecialBuildingProgress += used;
            kingdom.Budulec -= used;

            if (kingdom.SpecialBuildingProgress >= kingdom.SpecialBuildingCost)
            {
                var building = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == kingdom.CurrentSpecialBuilding);
                if (building != null)
                {
                    building.Quantity = 1;
                    building.IsUnderConstruction = false;
                }
                kingdom.CurrentSpecialBuilding = null;
                kingdom.SpecialBuildingProgress = 0;
                kingdom.SpecialBuildingCost = 0;
            }
        }

        long budulecLimit = 7500 + kingdom.Land / 4;
        kingdom.BudulecStored = Math.Min(kingdom.BudulecStored + kingdom.Budulec, budulecLimit);
        kingdom.Budulec = 0;

        // === 6. Popularność (oryginalny algorytm: cel = 2 × płace) ===
        int popularity = kingdom.Popularity;

        // a) +1 za każdy stojący budynek specjalny
        int specialsStanding = kingdom.Buildings
            .Count(b => b.Definition != null && b.Definition.IsSpecial && b.Quantity > 0 && !b.IsUnderConstruction);
        popularity += Math.Min(specialsStanding, 5); // ograniczenie, by nie eksplodowało

        // e) zbliżanie do dwukrotności płac: ±(1 + |2·płace − pop|/10)
        int target = Math.Min(100, kingdom.Wages * 2);
        if (popularity < target)
            popularity += 1 + (target - popularity) / 10;
        else if (popularity > target)
            popularity -= 1 + (popularity - target) / 10;

        // f) −15, jeśli brakło złota na pensje
        if (kingdom.Gold < 0)
        {
            popularity -= 15;
            kingdom.Gold = 0;
        }

        kingdom.Popularity = Math.Clamp(popularity, 0, 100);

        // === 7. Maksimum ludności (oryginalny wzór) ===
        // max = domy·(do+vo+ns+kn) + ziemia·(1 + (pp/100)·(2+vd+rb))
        // Mapowanie budynków „wodnych": Łaźnia miejska→Wodociągi(+0,5 dom),
        // System jaskiń→System nor(+1 dom), Kanalizacja→Kanalizacja(+1,5 dom),
        // Akwedukt→Wodotok(+0,5 akr).
        var houses = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "Domy");
        decimal houseCap = race.HouseCapacityBase
            + (Has("LazniaMiejska") ? race.WaterworksHouseBonus : 0m)
            + (Has("SystemJaskin") ? race.BurrowsHouseBonus : 0m)
            + (Has("Kanalizacja") ? race.SewersHouseBonus : 0m);
        decimal acreBonus = race.PopPerAcreBase - 3m; // rb względem standardu 3/akr
        decimal vd = Has("Akwedukt") ? race.AqueductAcreBonus : 0m;
        decimal popPct = kingdom.Popularity / 100m;

        long populationCap = (long)((houses?.Quantity ?? 0) * houseCap
            + kingdom.Land * (1m + popPct * (2m + vd + acreBonus)));
        populationCap = Math.Max(100, populationCap);

        // === 8. Przyrost / ubytek ludności ===
        if (kingdom.Population > populationCap)
        {
            // ubytek = (1 + nadwyżka·0,333/pojemność), max 33% na turę
            long surplus = kingdom.Population - populationCap;
            decimal lossPct = Math.Min(0.33m, 0.01m + surplus * 0.3333m / populationCap);
            kingdom.Population -= (int)(kingdom.Population * lossPct);
        }
        else
        {
            long freeSpace = populationCap - kingdom.Population;
            if (freeSpace > 0)
            {
                // przyrost = wolne·(profesje + bezrobotni + 0,75·armia)/3/pojemność, min 10% wolnego
                decimal growth = freeSpace
                    * (totalWorkers + GetUnemployed(kingdom) + 0.75m * totalSoldiers)
                    / 3m / populationCap;
                growth = Math.Max(growth, freeSpace * 0.10m);
                growth *= (1m + race.PopGrowthModifier);
                kingdom.Population += (int)growth;
                if (kingdom.Population > populationCap)
                    kingdom.Population = (int)populationCap;
            }
        }

        // === 9. Szkolenie nowicjuszy ===
        // p% = 100/(6 − 500·s/(z + 100·s + 99)) — s: liczba szkół
        var schools = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "Szkoly" && !b.IsUnderConstruction);
        decimal s = schools?.Quantity ?? 0;
        decimal trainedPct = 100m / (6m - 500m * s / (kingdom.Land + 100m * s + 99m)) / 100m;
        foreach (var prof in kingdom.Professions.Where(p => p.NoviceCount > 0))
        {
            int trained = Math.Max(1, (int)(prof.NoviceCount * trainedPct));
            prof.NoviceCount = Math.Max(0, prof.NoviceCount - trained);
            prof.NovicePercent = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100
                : 0;
        }

        // === 10. Mana po turze ===
        // Mana znika po turze; wyjątek — Dżin przechowuje 1 manę na mieszkańca
        if (kingdom.Race == "Dżin")
        {
            kingdom.Mana = Math.Min(kingdom.Mana, kingdom.Population);
        }
        else
        {
            kingdom.Mana = 0;
        }

        kingdom.TurnNumber++;
    }

    /// <summary>Limit SP/turę: baza wg poziomu Wynalazczości × modyfikator rasy.</summary>
    private async Task<long> ScienceCapAsync(Kingdom kingdom)
    {
        int devLevel = await _context.Researches.CountAsync(r =>
            r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Wynalazki"));
        long cap = DevelopmentCaps[Math.Min(devLevel, DevelopmentCaps.Length - 1)];
        decimal mult = kingdom.Race switch { "Człowiek" => 1.33m, "Goblin" => 0.8m, _ => 1m };
        return (long)(cap * mult);
    }

    /// <summary>
    /// Inwestuje SP w aktualnie rozwijaną dziedzinę; po osiągnięciu progu kończy badanie,
    /// a nadwyżkę odkłada do zapasu (SciencePoints). Bez wybranej dziedziny SP idą do zapasu.
    /// </summary>
    private async Task InvestScienceAsync(Kingdom kingdom, long sp)
    {
        if (sp <= 0) return;

        if (string.IsNullOrEmpty(kingdom.CurrentResearchTech))
        {
            kingdom.SciencePoints += sp;
            return;
        }

        var research = await _context.Researches
            .FirstOrDefaultAsync(r => r.KingdomId == kingdom.Id && r.TechType == kingdom.CurrentResearchTech);
        var tech = await _context.TechnologyDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TechType == kingdom.CurrentResearchTech);

        if (research == null || tech == null || research.IsCompleted)
        {
            kingdom.CurrentResearchTech = null;
            kingdom.SciencePoints += sp;
            return;
        }

        // Dorzucamy zgromadzony zapas do bieżącej inwestycji
        long pool = sp + kingdom.SciencePoints;
        kingdom.SciencePoints = 0;
        research.InvestedScience += pool;

        if (research.InvestedScience >= tech.CostScience)
        {
            long leftover = research.InvestedScience - tech.CostScience;
            research.InvestedScience = tech.CostScience;
            research.IsCompleted = true;
            research.IsInProgress = false;
            research.CompletedAt = DateTime.UtcNow;
            kingdom.CurrentResearchTech = null;
            kingdom.SciencePoints = leftover;
        }
    }

    private static bool IsEliteTwo(MilitaryUnit unit)
    {
        // E2 rozpoznawane po typach jednostek elity 2. stopnia
        return unit.UnitType.EndsWith("_Paladyn") || unit.UnitType.EndsWith("_LesnaZjawa")
            || unit.UnitType.EndsWith("_Berserker") || unit.UnitType.EndsWith("_Nornik")
            || unit.UnitType.EndsWith("_Ghul") || unit.UnitType.EndsWith("_DzinBeam")
            || unit.UnitType.EndsWith("_SkurutHai") || unit.UnitType.EndsWith("_Drzewiec")
            || unit.UnitType.EndsWith("_Nosferatu") || unit.UnitType.EndsWith("_Niszczyciel");
    }

    private static int GetUnemployed(Kingdom kingdom)
    {
        return kingdom.Professions
            .FirstOrDefault(p => p.ProfessionType == "Bezrobotni")?.WorkerCount ?? 0;
    }

    public async Task GenerateResourcesForAllAsync()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .Where(k => k.Era.IsActive && !k.IsFrozen)   // zamrożone księstwa pomijamy
            .ToListAsync();

        foreach (var kingdom in kingdoms)
        {
            await GenerateResourcesForKingdomAsync(kingdom);
        }

        await _context.SaveChangesAsync();
    }
}
