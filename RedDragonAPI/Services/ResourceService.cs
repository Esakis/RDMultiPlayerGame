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

    // Bazowa produkcja na pracownika za turę.
    // Wg manuala (profese.txt): alchemik przy 100% produkuje 100 złota,
    // chłop 10 jedzenia. Pozostałe bazy przybliżone (relacje zachowane).
    private const long AlchemistGoldBase = 100;
    private const long FarmerFoodBase = 10;
    private const long DruidManaBase = 1;
    private const long StonemasonStoneBase = 5;
    private const long ArmorerWeaponsBase = 3;
    private const decimal MasonStonePerBudulec = 2m;

    // Badania (docs/MECHANIKA.md §13): bazowe SP na naukowca/turę oraz limit SP/turę
    // wg poziomu dziedziny „Wynalazczość": bazowo 20k, a po odkryciach limit ustala
    // efekt ScienceCap (35k / 50k / 100k / 125k / 150k — wartość najwyższego odkrycia).
    private const decimal ScientistSciencePerWorker = 25m;
    private const long BaseScienceCap = 20_000;

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

        // Aktywne czary (m.in. Dobry/Zły humor) — potrzebne do kroków (b)/(c) popularności
        if (kingdom.ActiveSpells == null || !kingdom.ActiveSpells.Any())
        {
            await _context.Entry(kingdom)
                .Collection(k => k.ActiveSpells)
                .Query()
                .Include(s => s.Spell)
                .LoadAsync();
        }

        var race = await _context.RaceDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == kingdom.Race)
            ?? new RaceDefinition { Name = kingdom.Race };

        // Cechy główne generałów (docs/MECHANIKA.md §11) — liczy się najlepszy generał w domu:
        // Kupiec: +1,5·lvl/(lvl+50) produktywności kupców; Profesor: +lvl p.p. szkolenia nowicjuszy.
        var nowUtc = DateTime.UtcNow;
        var homeGenerals = await _context.Generals.AsNoTracking()
            .Where(g => g.KingdomId == kingdom.Id && !g.IsPending && !g.IsImprisoned && !g.IsOutside
                        && (g.WoundedUntil == null || g.WoundedUntil <= nowUtc)
                        && (g.PrimaryTrait == "Kupiec" || g.PrimaryTrait == "Profesor"))
            .ToListAsync();
        int merchantGeneralLvl = homeGenerals.Where(g => g.PrimaryTrait == "Kupiec")
            .Select(g => g.Level).DefaultIfEmpty(0).Max();
        int professorGeneralLvl = homeGenerals.Where(g => g.PrimaryTrait == "Profesor")
            .Select(g => g.Level).DefaultIfEmpty(0).Max();
        decimal merchantGeneralMult = 1m + 1.5m * merchantGeneralLvl / (merchantGeneralLvl + 50m);

        bool Has(string buildingType) => kingdom.Buildings
            .Any(b => b.BuildingType == buildingType && b.Quantity > 0 && !b.IsUnderConstruction);

        int Count(string buildingType) => kingdom.Buildings
            .Where(b => b.BuildingType == buildingType && !b.IsUnderConstruction)
            .Sum(b => b.Quantity);

        // Liczba cechów wzmacniających daną profesję (model 3 cechów po 2 profesje):
        // Cech słońca → alchemicy/chłopi, Cech ziemi → druidzi/kamieniarze,
        // Cech gwiazd → murarze/płatnerze. Kupcy/naukowcy/magowie nie mają cechu.
        int GuildCount(string professionType) => professionType switch
        {
            "Alchemicy" or "Chłopi" => Count("CechSlonca"),
            "Druidzi" or "Kamieniarze" => Count("CechZiemi"),
            "Murarze" or "Płatnerze" => Count("CechGwiazd"),
            _ => 0
        };

        // === 1. Produkcja profesji ===
        // produktywność = (100 − pn·0,9)/100 · (1+rb) · (1+pv) · (1+cech) · ...
        // pn — % nowicjuszy (nowicjusz pracuje na 10%), rb — bonus rasowy,
        // pv — wynalezienie (tu: Education z naukowców).
        // Bonusy badań: ogólna produkcja (Wynalazczość), handel (Rachunkowość), kamień (Górnictwo)
        decimal productionBonus = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "ProductionBonus");
        double scienceBonus = (double)await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "ScienceBonus");

        // Faza Księżyca (MECHANIKA §13) — Złoty sierp podwaja zysk z Kopalni złota
        var (moonPhase, bloodMoon) = await MoonPhaseHelper.GetAsync(_context);

        // Budynki specjalne ekonomii (Dracopedia §7.2 — Świątynia/Ołtarz/Monument ekonomii
        // odwzorowane jako nazwane budynki): bonusy % do produkcji wszystkich profesji.
        if (Has("SwiatyniaAutora")) productionBonus += 0.04m;   // Świątynia bogactwa Autora (świątynia +4%)
        if (Has("Mlyn")) productionBonus += 0.02m;              // Młyn (monument +2%)
        if (Has("KlubOdkrywcow")) productionBonus += 0.02m;     // Klub odkrywców (ołtarz +2%)
        // Górnictwo odkrywkowe: stabilny urobek złota = % złota alchemików (Dracopedia).
        decimal mineGoldRate = await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "MineGold");
        // Inżynieria zaawansowana: murarze zużywają 10% mniej kamienia.
        // U Elfa poziom 4 działa zamiast tego jako rabat 32% na zabudowania (Dracopedia).
        bool masonsStoneSaver = kingdom.Race != "Elf" && await _context.Researches.AnyAsync(r =>
            r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType == "Inzynieria4");

        decimal inventedBonus = 1m + (kingdom.Education / 100m) + productionBonus;

        // Zaklęcia produkcyjne (Dracopedia §9; wartości udokumentowane jako maksymalne).
        // Pracowitość/Somnambulizm działają na profesje niemagiczne, Fluid magiczny/Głupota
        // na magiczne (magowie, druidzi). Buffy są zawieszone na rzucającym, debuffy na celu.
        bool HasSpell(string spellType) => kingdom.ActiveSpells.Any(s => s.SpellType == spellType);
        decimal nonMagicSpellMult = Math.Max(0m, 1m
            + (HasSpell("Pracowitosc") ? 0.49m : 0m)
            - (HasSpell("Somnambulizm") ? 0.50m : 0m));
        decimal magicSpellMult = Math.Max(0m, 1m
            + (HasSpell("FluidMagiczny") ? 0.49m : 0m)
            - (HasSpell("Glupota") ? 0.25m : 0m));
        static bool IsMagicProfession(string type) => type is "Druidzi" or "Magowie";

        decimal Productivity(Profession prof, decimal raceBonus)
        {
            decimal novicePct = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100m
                : 0m;
            decimal baseEff = (100m - novicePct * 0.9m) / 100m;

            // Bonus cechu (Dracopedia §3): proc = int(100·(ce/(pr·0,08+ce+99))),
            // ce — liczba cechów profesji, pr — liczba pracowników profesji.
            decimal guildBonus = 0m;
            int ce = GuildCount(prof.ProfessionType);
            if (ce > 0 && prof.WorkerCount > 0)
                guildBonus = (int)(100m * ((decimal)ce / (prof.WorkerCount * 0.08m + ce + 99m))) / 100m;

            decimal spellMult = IsMagicProfession(prof.ProfessionType) ? magicSpellMult : nonMagicSpellMult;

            return baseEff * (1m + raceBonus) * (1m + guildBonus) * spellMult * inventedBonus;
        }

        // Bonus uniwersytetów dla naukowców (Dracopedia §3):
        // proc = int(100·(un/(pr/3·0,08+un+99))), un — liczba uniwersytetów,
        // pr — wszyscy pracownicy w profesjach (z naukowcami, bez bezrobotnych).
        int universities = Count("Uniwersytety");
        decimal universityBonus = 0m;
        if (universities > 0)
        {
            long prAll = kingdom.Professions
                .Where(p => p.ProfessionType != "Bezrobotni")
                .Sum(p => (long)p.WorkerCount);
            universityBonus = (int)(100m * (universities / (prAll / 3m * 0.08m + universities + 99m))) / 100m;
        }

        long merchantGold = 0;
        // Mana nie jest dodawana wprost — druidzi (i Manowe jeziorko / Dżin) wyznaczają
        // POJEMNOŚĆ many, do której manę uzupełnia się o ⅓ różnicy w sekcji 10 (profese.txt).
        long manaCapacity = 0;
        foreach (var prof in kingdom.Professions)
        {
            long production = 0;

            switch (prof.ProfessionType)
            {
                case "Alchemicy":
                    production = (long)(prof.WorkerCount * AlchemistGoldBase * Productivity(prof, race.BonusAlchemists));
                    kingdom.Gold += production;
                    // Kopalnia złota (Dracopedia §14.3): ~10% szansy na turę, że górnicy
                    // trafią na żyłę wartą 80–160% turowej produkcji złota alchemików;
                    // Złoty sierp (MECHANIKA §13) podwaja szansę. Górnictwo odkrywkowe
                    // zmienia kopalnię w mniejszy, ale stabilny urobek (MineGold).
                    if (Has("KopalniaZlota"))
                    {
                        if (mineGoldRate > 0)
                        {
                            kingdom.Gold += (long)(production * mineGoldRate);
                        }
                        else
                        {
                            double mineChance = 0.10;
                            if (moonPhase == MoonPhaseHelper.ZlotySierp
                                && MoonPhaseHelper.Affects(kingdom.Race, moonPhase, bloodMoon))
                                mineChance *= 2;
                            if (Random.Shared.NextDouble() < mineChance)
                            {
                                long found = (long)(production
                                    * (0.8m + (decimal)Random.Shared.NextDouble() * 0.8m));
                                kingdom.Gold += found;
                                _context.KingdomEvents.Add(new KingdomEvent
                                {
                                    KingdomId = kingdom.Id,
                                    Category = "Economy",
                                    Message = $"Górnicy natrafili na żyłę złota: +{found:N0} złota."
                                });
                            }
                        }
                    }
                    break;
                case "Chłopi":
                    production = (long)(prof.WorkerCount * FarmerFoodBase * Productivity(prof, race.BonusFarmers));
                    kingdom.Food += production;
                    break;
                case "Druidzi":
                    // Druidzi nie produkują many wprost — ich liczba × produktywność to
                    // POJEMNOŚĆ many, jaką księstwo może utrzymać (profese.txt: „1 mana/druida”).
                    production = (long)(prof.WorkerCount * DruidManaBase * Productivity(prof, race.BonusDruids));
                    manaCapacity += production;
                    break;
                case "Kamieniarze":
                    production = (long)(prof.WorkerCount * StonemasonStoneBase * Productivity(prof, race.BonusStonemasons));
                    kingdom.Stone += production;
                    break;
                case "Murarze":
                    // Murarze przerabiają kamień na budulec (infrapunkty).
                    // Inżynieria zaawansowana: −10% zużycia kamienia.
                    decimal stonePerBudulec = MasonStonePerBudulec * (masonsStoneSaver ? 0.9m : 1m);
                    long stoneNeeded = (long)(prof.WorkerCount * stonePerBudulec);
                    long stoneUsed = Math.Min(stoneNeeded, kingdom.Stone);
                    production = (long)(stoneUsed / stonePerBudulec * Productivity(prof, race.BonusMasons));
                    kingdom.Stone -= stoneUsed;
                    kingdom.Budulec += production;
                    break;
                case "Płatnerze":
                    production = (long)(prof.WorkerCount * ArmorerWeaponsBase * Productivity(prof, race.BonusArmorers));
                    kingdom.Weapons += production;
                    break;
                case "Kupcy":
                    // Oryginalny wzór: złoto na kupca = 500·z/(z + ob·10),
                    // z — obszar własny + obszar partnerów paktu HANDLOWEGO.
                    // Pakt handlowy jest jednym z 4 typów paktu (urza-pakt.txt) — aby
                    // doliczyć obszar sojusznika do handlu, trzeba mieć z nim aktywny pakt handlowy.
                    if (prof.WorkerCount > 0)
                    {
                        long tradeLand = kingdom.Land;
                        if (kingdom.CoalitionId != null)
                        {
                            var tradePartnerIds = await _context.Pacts
                                .Where(p => p.Status == "Active" && p.PactType == "Handlowy"
                                            && (p.ProposerKingdomId == kingdom.Id || p.TargetKingdomId == kingdom.Id))
                                .Select(p => p.ProposerKingdomId == kingdom.Id
                                    ? p.TargetKingdomId
                                    : p.ProposerKingdomId)
                                .Distinct()
                                .ToListAsync();

                            if (tradePartnerIds.Count > 0)
                                tradeLand += await _context.Kingdoms
                                    .Where(k => tradePartnerIds.Contains(k.Id))
                                    .SumAsync(k => (long)k.Land);
                        }
                        decimal goldPerMerchant = 500m * tradeLand / (tradeLand + prof.WorkerCount * 10m);
                        production = (long)(prof.WorkerCount * goldPerMerchant
                                            * Productivity(prof, race.BonusMerchants)
                                            * merchantGeneralMult);
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
                                        * Productivity(prof, race.BonusScientists)
                                        * (1m + universityBonus);
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

        // === 2. Produkcja manufaktur (Dracopedia §7.1) ===
        // p = (z/(z+m·25))·k·(1+2·inżynieria/100); k: Sad owocowy 400 (jedzenie),
        // Kamieniołom 40 (kamień), Kopalnia diamentów 4000 (złoto), Manowe jeziorko 40 (mana).
        int inzynieriaLevel = await _context.Researches.CountAsync(r =>
            r.KingdomId == kingdom.Id && r.IsCompleted && r.TechType.StartsWith("Inzynieria"));
        decimal manuBonus = 1m + 0.02m * inzynieriaLevel;

        void Manufactory(string type, decimal k, Action<long> add)
        {
            var b = kingdom.Buildings.FirstOrDefault(x => x.BuildingType == type && x.Quantity > 0 && !x.IsUnderConstruction);
            if (b == null) return;
            decimal m = b.Quantity;
            decimal per = kingdom.Land / (kingdom.Land + m * 25m) * k * manuBonus;
            add((long)(per * m));
        }
        Manufactory("Manufaktura", 400m, v => kingdom.Food += v);        // Sad owocowy (jedzenie)
        Manufactory("Kamieniolom", 40m, v => kingdom.Stone += v);        // Kamieniołom (kamień)
        Manufactory("KopalniaDiamentow", 4000m, v => kingdom.Gold += v); // Kopalnia diamentów (złoto)
        Manufactory("ManoweJeziorko", 40m, v => manaCapacity += v);      // Manowe jeziorko (zwiększa pojemność many)

        // Ratusz (budynek specjalny, Dracopedia §14.3): podatek 10 zł/mieszkańca na turę
        // (Ludzie 20 zł). Wojsko i złodzieje nie są częścią ludności cywilnej,
        // więc podatek ich nie obejmuje.
        if (Has("Ratusz")) kingdom.Gold += kingdom.Population * (kingdom.Race == "Człowiek" ? 20L : 10L);

        // Port towarowy (Dracopedia §14.3): 400–600 tys. złota na turę, w czasie wojny ×2.
        if (Has("PortTowarowy"))
        {
            long portGold = Random.Shared.Next(400_000, 600_001);
            if (kingdom.CoalitionId != null && await _context.Wars.AnyAsync(w => w.Status == "Active"
                    && (w.DeclaringCoalitionId == kingdom.CoalitionId || w.TargetCoalitionId == kingdom.CoalitionId)))
                portGold *= 2;
            kingdom.Gold += portGold;
        }

        // === 3. Pensje i żołd ===
        // Nowicjusze produkują tylko 10% i NIE pobierają pensji (są na nauce zawodu) —
        // dzięki temu rozbudowa zatrudnienia nie topi nowego księstwa w długach, zanim
        // pracownicy się wyszkolą. Pełną stawkę płacą dopiero wyszkoleni.
        int totalWorkers = kingdom.Professions
            .Where(p => p.ProfessionType != "Bezrobotni")
            .Sum(p => p.WorkerCount);
        long noviceWorkers = kingdom.Professions
            .Where(p => p.ProfessionType != "Bezrobotni")
            .Sum(p => (long)p.NoviceCount);
        long trainedWorkers = totalWorkers - noviceWorkers;
        long wagesCost = trainedWorkers * kingdom.Wages;
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
        // ludność je wg rasy (Olbrzym 2); armia je 1 (Nekromant — armia nieumarłych nie je)
        // Hodokvas Hobbita: w czasie uczty ludność je 5 jednostek na osobę
        bool armyEats = kingdom.Race != "Nekromant";
        int totalSoldiers = militaryUnits
            .Where(u => !u.UnitType.EndsWith("_Smok"))
            .Sum(u => u.Quantity);
        decimal foodPerPop = kingdom.HodokvasActive ? 5m : race.FoodPerPop;
        long foodNeeded = (long)(kingdom.Population * foodPerPop)
                          + (armyEats ? totalSoldiers : 0);
        kingdom.Food -= foodNeeded;

        // Krok (d) popularności: kara za niedobór jedzenia, −1…−15 wg skali niedoboru.
        // Wartość wyliczona tu, ale zastosowana niżej w sekcji 6 (zgodnie z kolejnością manuala).
        int foodShortagePenalty = 0;
        if (kingdom.Food < 0)
        {
            long deficit = -kingdom.Food;
            decimal severity = foodNeeded > 0 ? Math.Min(1m, (decimal)deficit / foodNeeded) : 1m;
            foodShortagePenalty = Math.Clamp((int)Math.Ceiling(severity * 15m), 1, 15);

            kingdom.Food = 0;
            // Głód: ludzie umierają/uciekają (kara popularności w sekcji 6)
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

        // Br-Oug (§2.2): podwójny limit infrapunktów (rekompensuje droższe budynki)
        long budulecLimit = (7500 + kingdom.Land / 4) * (kingdom.Race == "Br-Oug" ? 2 : 1);
        kingdom.BudulecStored = Math.Min(kingdom.BudulecStored + kingdom.Budulec, budulecLimit);
        kingdom.Budulec = 0;

        // === 6. Popularność (oryginalny algorytm z manuala, kolejno a→f; bazowo cel = 2 × płace) ===
        bool wagesUnpaid = kingdom.Gold < 0;

        // Hodokvas Hobbita: popularność w czasie uczty nie rośnie (może przekraczać 100),
        // spada tylko z kar za brak jedzenia/złota.
        if (kingdom.HodokvasActive)
        {
            int hodokvasPop = kingdom.Popularity - foodShortagePenalty;
            if (wagesUnpaid)
            {
                hodokvasPop -= 15;
                kingdom.Gold = 0;
            }
            kingdom.Popularity = Math.Max(0, hodokvasPop);
            kingdom.HodokvasTurnsPlayed++;
        }
        else
        {
            int popularity = kingdom.Popularity;

            // a) +1 za każdy stojący budynek specjalny (bez limitu — zgodnie z manualem)
            int specialsStanding = kingdom.Buildings
                .Count(b => b.Definition != null && b.Definition.IsSpecial && b.Quantity > 0 && !b.IsUnderConstruction);
            popularity += specialsStanding;

            // b) Dobry humor: +1 za każdy aktywny czar; c) Zły humor: −1 za każdy aktywny czar
            popularity += kingdom.ActiveSpells.Count(s => s.Spell != null && s.Spell.EffectType == "PopularityBuff");
            popularity -= kingdom.ActiveSpells.Count(s => s.Spell != null && s.Spell.EffectType == "PopularityDebuff");

            // d) niedobór jedzenia: −1…−15 wg skali (policzone w sekcji 4)
            popularity -= foodShortagePenalty;

            // e) zbliżanie do celu: ±(1 + |cel − pop|/10).
            //    Cel to 2× płace; Zajazd u Czerwonego Smoka obniża próg — płaca 42 daje 100%.
            int target = Has("ZajazdCzerwonego")
                ? Math.Min(100, (int)Math.Round(kingdom.Wages * 100m / 42m))
                : Math.Min(100, kingdom.Wages * 2);
            if (popularity < target)
                popularity += 1 + (target - popularity) / 10;
            else if (popularity > target)
                popularity -= 1 + (popularity - target) / 10;

            // f) −15, jeśli brakło złota na pensje
            if (wagesUnpaid)
            {
                popularity -= 15;
                kingdom.Gold = 0;
            }

            kingdom.Popularity = Math.Clamp(popularity, 0, 100);
        }

        // === Dezercja: niezapłacony żołd lub niska popularność = ucieczka armii ===
        // (Nekromant — armia nieumarłych nie dezerteruje; smoki zostają)
        if (kingdom.Race != "Nekromant")
        {
            decimal desertRate = 0m;
            if (wagesUnpaid) desertRate += 0.10m;                       // brak żołdu: −10%
            if (kingdom.Popularity < 20) desertRate += 0.05m;          // bunt z biedy: −5%
            else if (kingdom.Popularity < 40) desertRate += 0.02m;

            if (desertRate > 0)
            {
                foreach (var unit in militaryUnits.Where(u => u.Quantity > 0 && !u.UnitType.EndsWith("_Smok")))
                    unit.Quantity = Math.Max(0, unit.Quantity - (int)(unit.Quantity * desertRate));
            }
        }

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

                // Zamtuz pod Smoczym Ogonem (Dracopedia §14.3): +25% przyrostu nowych poddanych
                if (Has("Zamtuz")) growth *= 1.25m;

                // Hodokvas Hobbita: +50% przyrostu w czasie uczty
                if (kingdom.HodokvasActive) growth *= 1.5m;

                // Zaklęcia przyrostu (Dracopedia §4): Płodność ×1,3, Szczęście ×1,1,
                // Pech ×0,9, Kastracja ×0,5 (zawieszone buffy/debuffy mnożą się).
                // Nekromant jest odporny na Płodność i Kastrację (§2.2 — nieumarli).
                bool undead = kingdom.Race == "Nekromant";
                if (HasSpell("Plodnosc") && !undead) growth *= 1.3m;
                if (HasSpell("Szczescie")) growth *= 1.1m;
                if (HasSpell("Pech")) growth *= 0.9m;
                if (HasSpell("Kastracja") && !undead) growth *= 0.5m;

                kingdom.Population += (int)growth;
                if (kingdom.Population > populationCap)
                    kingdom.Population = (int)populationCap;
            }
        }

        // === 8b. Uzgodnienie zatrudnienia z ludnością ===
        // Suma pracowników we wszystkich profesjach (z bezrobotnymi) musi równać się
        // ludności. Gdy ludność spadła (głód/emigracja), proporcjonalnie ucinamy
        // pracowników w zawodach (odpływ). Gdy wzrosła — nadwyżka zasila bezrobotnych.
        ReconcileWorkforceWithPopulation(kingdom);

        // === 9. Szkolenie nowicjuszy ===
        // p% = 100/(3,5 − 250·s/(z + 100·s + 99)) — s: liczba szkół.
        // Baza (bez szkół) ≈ 28,6%/turę zamiast dawnych 16,7% — rampa nowicjusza trwa
        // ~3-4 tury zamiast ~13, więc nowe księstwo szybko osiąga pełną produktywność.
        // Szkoły skracają ją dalej (mianownik dąży do 1 → ~90%). Clamp dla bezpieczeństwa.
        var schools = kingdom.Buildings.FirstOrDefault(b => b.BuildingType == "Szkoly" && !b.IsUnderConstruction);
        decimal s = schools?.Quantity ?? 0;
        decimal trainedDenom = 3.5m - 250m * s / (kingdom.Land + 100m * s + 99m);
        // Generał Profesor: +lvl punktów procentowych szkolenia na turę (docs/MECHANIKA.md §11)
        decimal trainedPct = Math.Clamp(100m / trainedDenom / 100m + professorGeneralLvl / 100m, 0.05m, 0.95m);
        // Hodokvas Hobbita: szkolenie spada o 40% (względem aktualnej wartości)
        if (kingdom.HodokvasActive) trainedPct *= 0.6m;
        foreach (var prof in kingdom.Professions.Where(p => p.NoviceCount > 0))
        {
            int trained = Math.Max(1, (int)(prof.NoviceCount * trainedPct));
            prof.NoviceCount = Math.Max(0, prof.NoviceCount - trained);
            prof.NovicePercent = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100
                : 0;
        }

        // === 10. Mana po turze (profese.txt: druidzi wyznaczają pojemność many) ===
        // Mana NIE znika — co turę dochodzi ⅓ różnicy między posiadaną maną a pojemnością.
        // Gdy mana > pojemność (np. kupiona), nadwyżka maleje o ⅓ różnicy w stronę pojemności.
        // Dżin przechowuje dodatkowo 1 manę/mieszkańca; Elf many nie traci (może gromadzić ponad pojemność).
        if (kingdom.Race == "Dżin")
            manaCapacity += kingdom.Population;

        long manaDiff = manaCapacity - kingdom.Mana;
        long manaDelta = manaDiff / 3;
        // Domknij niewielką resztę, by mana nie utykała tuż pod/nad pojemnością.
        if (manaDelta == 0 && manaDiff != 0) manaDelta = manaDiff;
        // Elf many nie traci — przy nadwyżce nie zmniejszamy.
        if (kingdom.Race == "Elf") manaDelta = Math.Max(0, manaDelta);
        kingdom.Mana = Math.Max(0, kingdom.Mana + manaDelta);

        kingdom.TurnNumber++;
    }

    /// <summary>Limit SP/turę: baza wg poziomu Wynalazczości × modyfikator rasy.</summary>
    private async Task<long> ScienceCapAsync(Kingdom kingdom)
    {
        long researched = (long)await ResearchEffects.MaxEffectAsync(_context, kingdom.Id, "ScienceCap");
        long cap = Math.Max(BaseScienceCap, researched);
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

            // Czas (Zakrzywienie/Załamanie): jednorazowy zastrzyk tur w chwili odkrycia.
            // Zakrzywienie działa tylko do 10. dnia wieku księstwa.
            if (tech.EffectType == "StartTurns"
                && (tech.TechType != "ZakrzywCzasu" || kingdom.Age <= 10))
            {
                kingdom.TurnsAvailable += (int)tech.EffectValue;
            }
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

    /// <summary>
    /// Utrzymuje niezmiennik: Σ pracowników (wraz z bezrobotnymi) = ludność.
    /// Spadek ludności → proporcjonalne ucięcie pracowników (i nowicjuszy) we wszystkich
    /// zawodach. Wzrost → nadwyżka trafia do bezrobotnych.
    /// </summary>
    private static void ReconcileWorkforceWithPopulation(Kingdom kingdom)
    {
        long totalWorkers = kingdom.Professions.Sum(p => (long)p.WorkerCount);
        long population = kingdom.Population;

        if (totalWorkers == population) return;

        if (population > totalWorkers)
        {
            // Nowi mieszkańcy zasilają bezrobotnych.
            var unemployed = kingdom.Professions.FirstOrDefault(p => p.ProfessionType == "Bezrobotni");
            if (unemployed != null)
                unemployed.WorkerCount += (int)(population - totalWorkers);
            return;
        }

        // Ludność spadła — proporcjonalny odpływ z każdego zawodu.
        long toRemove = totalWorkers - population;
        long removed = 0;
        foreach (var prof in kingdom.Professions)
        {
            if (prof.WorkerCount <= 0) continue;
            int cut = (int)((long)prof.WorkerCount * toRemove / totalWorkers);
            cut = Math.Min(cut, prof.WorkerCount);
            prof.WorkerCount -= cut;
            removed += cut;
            // Nowicjusze nie mogą przewyższać liczby pracowników.
            if (prof.NoviceCount > prof.WorkerCount) prof.NoviceCount = prof.WorkerCount;
        }

        // Reszta z zaokrągleń — ucinamy od najliczniejszych zawodów.
        long remainder = toRemove - removed;
        while (remainder > 0)
        {
            var biggest = kingdom.Professions
                .Where(p => p.WorkerCount > 0)
                .OrderByDescending(p => p.WorkerCount)
                .FirstOrDefault();
            if (biggest == null) break;
            biggest.WorkerCount--;
            if (biggest.NoviceCount > biggest.WorkerCount) biggest.NoviceCount = biggest.WorkerCount;
            remainder--;
        }

        // Odśwież % nowicjuszy.
        foreach (var prof in kingdom.Professions)
            prof.NovicePercent = prof.WorkerCount > 0
                ? (decimal)prof.NoviceCount / prof.WorkerCount * 100
                : 0;
    }

    public async Task GenerateResourcesForAllAsync()
    {
        var kingdoms = await _context.Kingdoms
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .Where(k => k.Era.IsActive && !k.IsFrozen && !k.IsSuspended)   // zamrożone księstwa pomijamy
            .ToListAsync();

        foreach (var kingdom in kingdoms)
        {
            await GenerateResourcesForKingdomAsync(kingdom);
        }

        await _context.SaveChangesAsync();
    }
}
