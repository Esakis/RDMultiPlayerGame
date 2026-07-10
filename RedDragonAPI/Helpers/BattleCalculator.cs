using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Wzory walki wg oryginalnego Red Dragon (manual, strona „vzorce" —
/// szczegóły i oznaczenia: docs/MECHANIKA.md §8).
///
/// Atak:   u = { [armia z bonusami budynków] · (1 + r/(50+r)) + r·100 + machiny·x }
///             · (1 + 0,1·NamiotDowodcy)
/// Obrona: o = { [armia + domobrana + wieże] · (1 + r/(50+r)) + r·100 }
///             · (1+0,05·Szaniec) · (1+0,1·SmoczyMur) · (1+0,1·SmoczaBariera) · (1+0,15·Zamek)
/// Straty: ~15% przy równowadze, 0–30% wg przewagi, modyfikatory rasowe.
/// </summary>
public static class BattleCalculator
{
    private static readonly Random Random = new();

    private static bool Has(Kingdom k, string buildingType) =>
        k.Buildings != null && k.Buildings.Any(b =>
            b.BuildingType == buildingType && b.Quantity > 0 && !b.IsUnderConstruction);

    private static int Count(Kingdom k, string buildingType) =>
        k.Buildings?.FirstOrDefault(b => b.BuildingType == buildingType && !b.IsUnderConstruction)?.Quantity ?? 0;

    private static bool IsMachine(string unitType) => unitType.EndsWith("_Machina");
    private static bool IsDragon(string unitType) => unitType.EndsWith("_Smok");
    private static bool IsThief(string unitType) => unitType.EndsWith("_Zlodziej");
    private static bool IsHoplite(string unitType) => unitType.EndsWith("_Hoplita");
    private static bool IsElite1(UnitDefinition def) => def.RequiredBuilding == "OltarzInicjacji";
    private static bool IsElite2(UnitDefinition def) => def.RequiredBuilding == "KoszarySpecjalne";

    /// <summary>Siła ataku wysłanej armii (oryginalny wzór, bez generałów).</summary>
    public static long CalculateAttackPower(
        Kingdom attacker,
        Dictionary<string, int> sentUnits,
        RaceDefinition attackerRace)
    {
        // c = 1, gdy stoi Sanktuarium berserkerów (Cvičiště berserkrovství)
        decimal c = Has(attacker, "SanktuariumBerserkerow") ? 1m : 0m;

        // Pałac: +1 ataku dla elity 2. stopnia; Gildia Wojowników (u Olbrzyma
        // zastępuje Gildię Złodziei): +1 ataku E2 (Dracopedia §14.3).
        // Dozbrojenie Krasnoluda: punkty +1 ataku E1/E2 kupione za broń (reset po przeliczeniu).
        decimal e1AtkBonus = attacker.RearmE1Attack;
        decimal e2AtkBonus = (Has(attacker, "Palac") ? 1m : 0m)
            + (attackerRace.Name == "Olbrzym" && Has(attacker, "GildiaZlodziei") ? 1m : 0m)
            + attacker.RearmE2Attack;

        decimal armyPower = 0;
        long dragons = 0;
        long machines = 0;
        decimal machineStrength = attackerRace.MachineAttack;

        // Machiny z E1 (manual „armada"/MECHANIKA §8): siła +50%
        // (Br-Oug wg cechy rasowej: z E1 siła 6 zamiast 8, czyli ×0,75).
        bool machinesWithE1 = sentUnits.Any(s => s.Value > 0 && attacker.MilitaryUnits.Any(u =>
            u.UnitType == s.Key && u.Definition != null && IsElite1(u.Definition) && u.Quantity > 0));
        if (machinesWithE1)
            machineStrength *= attackerRace.Name == "Br-Oug" ? 0.75m : 1.5m;

        foreach (var sent in sentUnits)
        {
            var unit = attacker.MilitaryUnits.FirstOrDefault(u => u.UnitType == sent.Key);
            if (unit?.Definition == null) continue;
            int count = Math.Min(sent.Value, unit.Quantity);
            if (count <= 0) continue;

            if (IsDragon(sent.Key)) { dragons += count; continue; }
            if (IsMachine(sent.Key)) { machines += count; continue; }
            if (IsThief(sent.Key)) continue; // złodzieje nie walczą w polu

            // jednostki: hoplita (1+c), elity (s+c); nowicjusze pominięci (wyszkoleni)
            armyPower += count * (unit.Definition.AttackPower + c
                + (IsElite1(unit.Definition) ? e1AtkBonus : 0m)
                + (IsElite2(unit.Definition) ? e2AtkBonus : 0m));
        }

        // smoki: mnożnik (1 + r/(50+r)) i +100 ataku za smoka
        decimal dragonMult = 1m + dragons / (50m + dragons);
        decimal total = armyPower * dragonMult + dragons * 100m + machines * machineStrength;

        // Namiot dowódcy / Sztab uderzeniowy: +10% ataku; Plac defilad: +5% ataku
        if (Has(attacker, "SztabUderzeniowy")) total *= 1.10m;
        if (Has(attacker, "PlacDefilad")) total *= 1.05m;

        return (long)total;
    }

    /// <summary>Pełna siła obrony księstwa (oryginalny wzór, bez generałów).</summary>
    public static long CalculateDefensePower(Kingdom defender, RaceDefinition defenderRace)
    {
        // k = 1, gdy stoi Klasztor Smoczych Mnichów
        decimal k = Has(defender, "KlasztorMnichow") ? 1m : 0m;

        // Gildia Wojowników (u Olbrzyma zastępuje Gildię Złodziei): +2 obrony E2.
        // Dozbrojenie Krasnoluda: punkty +1 obrony E1/E2 kupione za broń.
        decimal e1DefBonus = defender.RearmE1Defense;
        decimal e2DefBonus = (defenderRace.Name == "Olbrzym" && Has(defender, "GildiaZlodziei") ? 2m : 0m)
            + defender.RearmE2Defense;

        decimal armyPower = 0;
        long dragons = 0;

        foreach (var unit in defender.MilitaryUnits)
        {
            if (unit.Definition == null || unit.Quantity <= 0) continue;
            if (IsDragon(unit.UnitType)) { dragons += unit.Quantity; continue; }
            if (IsMachine(unit.UnitType)) continue;   // machiny nie bronią (wyjątek: Goblin w wieżach, niżej)
            if (IsThief(unit.UnitType)) continue;     // złodzieje bronią tylko przed złodziejami

            armyPower += unit.Quantity * (unit.Definition.DefensePower + k
                + (IsElite1(unit.Definition) ? e1DefBonus : 0m)
                + (IsElite2(unit.Definition) ? e2DefBonus : 0m));
        }

        // Domobrana (Pospolite ruszenie, Dracopedia): broni CAŁA ludność cywilna
        // (bez wymogu przeszkolenia) + złodzieje w domu, ze współczynnikiem 2 + k.
        // Wyjątki rasowe: Goblin 3, Olbrzym 2,5, Br-Oug 1,5, Gnom 1.
        if (Has(defender, "PospoliteRuszenie"))
        {
            decimal raceBonus = defenderRace.Name switch
            {
                "Goblin" => 1m,
                "Olbrzym" => 0.5m,
                "Br-Oug" => -0.5m,
                "Gnom" => -1m,
                _ => 0m
            };

            long homeThieves = defender.MilitaryUnits
                .Where(u => IsThief(u.UnitType)).Sum(u => (long)u.Quantity);

            armyPower += (defender.Population + homeThieves) * (2m + k + raceBonus);
        }

        // Wieże obronne: v·(10+nbr)·(1 + 4v/(v+400)); Człowiek 10, pozostali 15;
        // Br-Oug: wieże słabsze o 33% (manual „vzorce" wzór 16: ×(1−⅓))
        long towers = Count(defender, "WiezeObronne");
        if (towers > 0)
        {
            decimal towerBase = defenderRace.Name == "Człowiek" ? 10m : 15m;
            if (defenderRace.Name == "Br-Oug") towerBase *= 2m / 3m;
            armyPower += towers * towerBase * (1m + 4m * towers / (towers + 400m));

            // Hoplici w wieżach (manual „armada": do 3 hoplitów/wieżę, +5 obrony każdy)
            long hoplites = defender.MilitaryUnits
                .Where(u => IsHoplite(u.UnitType)).Sum(u => (long)u.Quantity);
            armyPower += Math.Min(hoplites, towers * 3) * 5m;

            // Goblińska inżynieria (docs/MECHANIKA.md §2.2): wieże Goblina mieszczą
            // po 10 machin, każda broni z siłą 100.
            if (defenderRace.Name == "Goblin")
            {
                long goblinMachines = defender.MilitaryUnits
                    .Where(u => IsMachine(u.UnitType))
                    .Sum(u => (long)u.Quantity);
                long machinesInTowers = Math.Min(goblinMachines, towers * 10);
                armyPower += machinesInTowers * 100m;
            }
        }

        // smoki
        decimal dragonMult = 1m + dragons / (50m + dragons);
        decimal total = armyPower * dragonMult + dragons * 100m;

        // budynki obronne (Dracopedia): Szaniec +5%, Smoczy mur +10%,
        // Smocza bariera +10%, Zamek +15%. Sieć fortec nie daje % obrony —
        // zmniejsza utratę ziemi (CalculateLandCaptured).
        if (Has(defender, "Szaniec")) total *= 1.05m;
        if (Has(defender, "SmoczyMur")) total *= 1.10m;
        if (Has(defender, "SmoczaBariera")) total *= 1.10m;
        if (Has(defender, "Zamek")) total *= 1.15m;

        // Tarcze bojowe (Dracopedia §8): Tarcza wojenna +24% obrony,
        // Słabość −24% obrony (zawieszone na obrońcy jako ActiveSpell).
        if (defender.ActiveSpells != null)
        {
            if (defender.ActiveSpells.Any(s => s.SpellType == "TarczaWojenna")) total *= 1.24m;
            if (defender.ActiveSpells.Any(s => s.SpellType == "Slabosc")) total *= 0.76m;
        }

        // Upojenie armii (akcja złodziejska, docs/MECHANIKA.md §10): −% obrony do przeliczenia
        if (defender.DrunkArmyPct > 0)
            total *= 1m - Math.Min(90, defender.DrunkArmyPct) / 100m;

        // Padłe legie (MECHANIKA §8): dodatkowa obrona = min(siła zaklęcia,
        // liczba wyszkolonych magów); u Dżina legie są 3× skuteczniejsze.
        var legie = defender.ActiveSpells?.FirstOrDefault(s => s.SpellType == "PadleLegiony");
        if (legie != null)
        {
            var magesProf = defender.Professions?.FirstOrDefault(p => p.ProfessionType == "Magowie");
            long trainedMages = magesProf == null ? 0 : Math.Max(0, magesProf.WorkerCount - magesProf.NoviceCount);
            long le = Math.Min(legie.Power, trainedMages);
            if (defenderRace.Name == "Dżin") le *= 3;
            total += le;
        }

        // Goblińska inżynieria: machiny Goblina z E2 obniżyły obronę celu
        // (kara naliczona przy wcześniejszych atakach w tym przeliczeniu)
        if (defender.SiegeDefensePenalty > 0)
            total = Math.Max(0, total - defender.SiegeDefensePenalty);

        return (long)total;
    }

    /// <summary>Losowość: ±5% (oryginał ograniczał rolę przypadku).</summary>
    public static double GetRandomFactor()
    {
        return 0.95 + (Random.NextDouble() * 0.10);
    }

    /// <summary>
    /// Zdobyta ziemia (Dracopedia, Sieć wojennych fortec): kolejne przechodzące
    /// ataki w tym przeliczeniu zabierają 10/10/8/6/4/2% obszaru obrońcy,
    /// a z Siecią fortec 6/6/6/4,5/3/1,5%. Hobbit w obronie traci ×0,82
    /// (odpowiednik dawnych 9% zamiast 11%).
    /// </summary>
    public static int CalculateLandCaptured(Kingdom defender, RaceDefinition defenderRace, int priorBreaches)
    {
        decimal[] withFort = { 6m, 6m, 6m, 4.5m, 3m, 1.5m };
        decimal[] withoutFort = { 10m, 10m, 8m, 6m, 4m, 2m };
        var seq = Has(defender, "SiecFortec") ? withFort : withoutFort;
        decimal pct = seq[Math.Clamp(priorBreaches, 0, seq.Length - 1)] / 100m;
        if (defenderRace.Name == "Hobbit") pct *= 0.82m;
        return (int)(defender.Land * pct);
    }

    /// <summary>Rabunek zasobów przy zwycięstwie (przybliżenie oryginału).</summary>
    public static ResourcesStolen CalculateResourcesStolen(Kingdom defender)
    {
        return new ResourcesStolen
        {
            Gold = (long)(defender.Gold * 0.10),
            Food = (long)(defender.Food * 0.10),
            Stone = (long)(defender.Stone * 0.10),
            Weapons = (long)(defender.Weapons * 0.10)
        };
    }

    /// <summary>
    /// Straty atakującego: ~15% przy równowadze; maleją do ~0% przy przewadze,
    /// rosną do ~30% przy porażce. Modyfikator rasowy (Krasnolud −25%, Ent −50%).
    /// </summary>
    public static Dictionary<string, int> CalculateCasualties(
        Dictionary<string, int> attackerUnits,
        long attackPower,
        long defensePower,
        bool attackerWins,
        RaceDefinition attackerRace)
    {
        double ratio = (double)attackPower / Math.Max(defensePower, 1);
        double casualtyRate = BaseCasualtyRate(ratio);
        casualtyRate *= 1.0 + (double)attackerRace.MilitaryLossModifier;

        var casualties = new Dictionary<string, int>();
        foreach (var unit in attackerUnits)
        {
            if (IsDragon(unit.Key)) continue; // smoki giną tylko od smokobójców
            int lost = (int)(unit.Value * casualtyRate);
            if (lost > 0)
                casualties[unit.Key] = lost;
        }

        return casualties;
    }

    public static Dictionary<string, int> CalculateDefenderCasualties(
        ICollection<MilitaryUnit> defenderUnits,
        long attackPower,
        long defensePower,
        bool attackerWins,
        RaceDefinition defenderRace)
    {
        // dla obrońcy stosunek odwrotny
        double ratio = (double)defensePower / Math.Max(attackPower, 1);
        double casualtyRate = BaseCasualtyRate(ratio);
        casualtyRate *= 1.0 + (double)defenderRace.MilitaryLossModifier;

        var casualties = new Dictionary<string, int>();
        foreach (var unit in defenderUnits)
        {
            if (unit.Quantity <= 0 || IsDragon(unit.UnitType) || IsThief(unit.UnitType)) continue;
            int lost = (int)(unit.Quantity * casualtyRate);
            if (lost > 0)
                casualties[unit.UnitType] = lost;
        }

        return casualties;
    }

    /// <summary>
    /// Bazowa stopa strat wg stosunku sił własnych do wroga:
    /// równowaga (1,0) → 15%; duża przewaga (≥2,0) → ~0%; duża słabość (≤0,5) → 30%.
    /// Pomiędzy punktami liniowo (zgodnie z opisem w manualu).
    /// </summary>
    private static double BaseCasualtyRate(double ownToEnemyRatio)
    {
        if (ownToEnemyRatio >= 2.0) return 0.01;
        if (ownToEnemyRatio >= 1.0) return 0.15 - (ownToEnemyRatio - 1.0) * 0.14;
        if (ownToEnemyRatio >= 0.5) return 0.15 + (1.0 - ownToEnemyRatio) * 0.30;
        return 0.30;
    }

    /// <summary>
    /// Straty cywilów: 25% strat armii bez domobrany; 50% przy udanej obronie
    /// z domobraną; pełne straty przy nieudanej obronie z domobraną.
    /// </summary>
    public static int CalculateCivilianLosses(
        Kingdom defender, double armyCasualtyRate, bool defenseHeld)
    {
        bool militia = Has(defender, "PospoliteRuszenie");
        double rate = !militia ? armyCasualtyRate * 0.25
            : defenseHeld ? armyCasualtyRate * 0.5
            : armyCasualtyRate;
        // Komando/Szaniec (Dracopedia: ten sam budynek pod różnymi nazwami er) —
        // straty cywilów −20%; efekty się nie kumulują.
        if (Has(defender, "Komando") || Has(defender, "Szaniec")) rate *= 0.8;
        // Zamek (Dracopedia): schronienie dla cywilów — straty ludności −10%
        if (Has(defender, "Zamek")) rate *= 0.9;
        return (int)(defender.Population * Math.Min(rate, 0.33));
    }

    /// <summary>
    /// Szansa przejścia akcji złodziejskiej w wojnie (oryginalna skala):
    /// stosunek 0,5→0%; 1→50%; 1,5→95%; 2→100% (liniowo między punktami).
    /// </summary>
    public static double ThiefSuccessChance(long attackThieves, long defenseThieves)
    {
        double ratio = (double)attackThieves / Math.Max(defenseThieves, 1);
        if (ratio <= 0.5) return 0.0;
        if (ratio <= 1.0) return (ratio - 0.5) * 1.0;          // 0 → 0,5
        if (ratio <= 1.5) return 0.5 + (ratio - 1.0) * 0.9;    // 0,5 → 0,95
        if (ratio <= 2.0) return 0.95 + (ratio - 1.5) * 0.1;   // 0,95 → 1,0
        return 1.0;
    }

    /// <summary>
    /// Szansa wykrycia złodziei poza wojną: 0,5→100%; 1→75%; 1,5→50%; 2→25%;
    /// 2,5→0%; minimum zawsze 5%.
    /// </summary>
    public static double ThiefDetectionChance(long attackThieves, long defenseThieves)
    {
        double ratio = (double)attackThieves / Math.Max(defenseThieves, 1);
        double chance;
        if (ratio <= 0.5) chance = 1.0;
        else if (ratio >= 2.5) chance = 0.0;
        else chance = 1.0 - (ratio - 0.5) * 0.5;
        return Math.Max(0.05, chance);
    }
}
