using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

public static class BattleCalculator
{
    private static readonly Random Random = new();

    public static int CalculateAttackPower(
        ICollection<MilitaryUnit> units,
        Dictionary<string, int> sentUnits,
        IEnumerable<Research> completedResearch)
    {
        int totalPower = 0;

        foreach (var sent in sentUnits)
        {
            var unit = units.FirstOrDefault(u => u.UnitType == sent.Key);
            if (unit?.Definition != null)
            {
                int count = Math.Min(sent.Value, unit.Quantity);
                totalPower += unit.Definition.AttackPower * count;
            }
        }

        // Bonusy z technologii broni
        decimal weaponryBonus = completedResearch
            .Where(r => r.TechType.StartsWith("Weaponry") && r.IsCompleted && r.Tech != null)
            .Sum(r => r.Tech.EffectValue);

        totalPower = (int)(totalPower * (1 + (double)weaponryBonus));

        return totalPower;
    }

    public static int CalculateDefensePower(
        Kingdom defender,
        IEnumerable<Research> completedResearch)
    {
        int totalPower = 0;

        foreach (var unit in defender.MilitaryUnits)
        {
            if (unit.Definition != null)
                totalPower += unit.Definition.DefensePower * unit.Quantity;
        }

        // Bonusy z murów
        var walls = defender.Buildings.FirstOrDefault(b => b.BuildingType == "Wall");
        if (walls != null)
        {
            totalPower = (int)(totalPower * (1 + walls.Level * 0.05));
        }

        // Bonusy z wież
        var towers = defender.Buildings.FirstOrDefault(b => b.BuildingType == "GuardTower");
        if (towers != null)
        {
            totalPower = (int)(totalPower * (1 + towers.Quantity * 0.03));
        }

        // Bonusy z technologii zbroi
        decimal armorBonus = completedResearch
            .Where(r => r.TechType.StartsWith("Armor") && r.IsCompleted && r.Tech != null)
            .Sum(r => r.Tech.EffectValue);

        totalPower = (int)(totalPower * (1 + (double)armorBonus));

        return totalPower;
    }

    public static double GetRandomFactor()
    {
        return 0.7 + (Random.NextDouble() * 0.6); // 0.7 - 1.3
    }

    public static int CalculateLandCaptured(int attackPower, int defensePower, int defenderLand)
    {
        double powerRatio = (double)attackPower / Math.Max(defensePower, 1);
        int captured = (int)(defenderLand * 0.1 * powerRatio);
        return Math.Min(captured, defenderLand / 2);
    }

    public static ResourcesStolen CalculateResourcesStolen(Kingdom defender)
    {
        return new ResourcesStolen
        {
            Gold = (long)(defender.Gold * 0.2),
            Food = (long)(defender.Food * 0.15),
            Wood = (long)(defender.Wood * 0.15),
            Stone = (long)(defender.Stone * 0.15),
            Iron = (long)(defender.Iron * 0.15)
        };
    }

    public static Dictionary<string, int> CalculateCasualties(
        Dictionary<string, int> attackerUnits,
        int attackPower,
        int defensePower,
        bool attackerWins)
    {
        var casualties = new Dictionary<string, int>();
        double casualtyRate = attackerWins ? 0.1 : 0.3;

        if (!attackerWins)
        {
            double ratio = (double)defensePower / Math.Max(attackPower, 1);
            casualtyRate = Math.Min(0.5, casualtyRate * ratio);
        }

        foreach (var unit in attackerUnits)
        {
            int lost = (int)(unit.Value * casualtyRate);
            if (lost > 0)
                casualties[unit.Key] = lost;
        }

        return casualties;
    }

    public static Dictionary<string, int> CalculateDefenderCasualties(
        ICollection<MilitaryUnit> defenderUnits,
        int attackPower,
        int defensePower,
        bool attackerWins)
    {
        var casualties = new Dictionary<string, int>();
        double casualtyRate = attackerWins ? 0.2 : 0.05;

        foreach (var unit in defenderUnits)
        {
            if (unit.Quantity > 0)
            {
                int lost = (int)(unit.Quantity * casualtyRate);
                if (lost > 0)
                    casualties[unit.UnitType] = lost;
            }
        }

        return casualties;
    }
}
