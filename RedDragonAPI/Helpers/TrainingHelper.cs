using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Mechanika szkolenia wojska (awans jednostek co turę) wg Dracopedii (gałąź nauki Trening).
/// Procent awansu na turę zależy od poziomu Treningu (0–5):
///   E0→E1: 0 / 1 / 2 / 4 / 5,5 / 7 %
///   E1→E2: 0 / 0,2 / 0,5 / 1,2 / 1,8 / 2,2 %
/// </summary>
public static class TrainingHelper
{
    private static readonly decimal[] SoldierPct = { 0m, 1m, 2m, 4m, 5.5m, 7m };       // E0→E1
    private static readonly decimal[] ElitePct   = { 0m, 0.2m, 0.5m, 1.2m, 1.8m, 2.2m }; // E1→E2

    /// <summary>Poziom nauki Trening (0–5) na podstawie ukończonych odkryć Trening1..Trening5.</summary>
    public static int TrainingLevel(IEnumerable<Research> researches)
    {
        int lvl = 0;
        for (int i = 1; i <= 5; i++)
            if (researches.Any(r => r.TechType == $"Trening{i}" && r.IsCompleted))
                lvl = i;
        return lvl;
    }

    public static decimal SoldierPromotePct(int level) => SoldierPct[Math.Clamp(level, 0, 5)];
    public static decimal ElitePromotePct(int level) => ElitePct[Math.Clamp(level, 0, 5)];

    // Sloty jednostek niezależne od rasy (po sufiksie typu / opisie definicji).
    public static bool IsHoplita(UnitDefinition d) => d.UnitType.EndsWith("_Hoplita");
    public static bool IsElite1(UnitDefinition d) => d.Description?.Contains("Elita 1") == true;
    public static bool IsElite2(UnitDefinition d) => d.Description?.Contains("Elita 2") == true;

    // Budynki wymagane do szkolenia danego stopnia.
    public const string SoldierBuilding = "OltarzInicjacji";   // E0→E1
    public const string EliteBuilding   = "KoszarySpecjalne";  // E1→E2
}
