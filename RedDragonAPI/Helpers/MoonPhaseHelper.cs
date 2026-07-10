using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>
/// Fazy Księżyca (Dracopedia: docs/zrodla/dracopedia_pojecia/fazy_ksiezyca.html,
/// docs/MECHANIKA.md §13). Sześć faz zmienia się cyklicznie co przeliczenie,
/// siódma — Krwawy Księżyc — losowo zastępuje Pełnię w danym cyklu i ma
/// wszystkie jej właściwości. Stan globalny w GameSettings (MoonPhase 0–5,
/// MoonBloodMoon 0/1). Br-Ougowie są odporni na efekty faz poza Nowiem
/// i Krwawym Księżycem.
/// </summary>
public static class MoonPhaseHelper
{
    public const int Now = 0;            // Nów — udane akcje złodziejskie ujawniają pakty
    public const int ZlotySierp = 1;     // Złoty sierp — zysk z Kopalni złota ×2
    public const int OkoSmoka = 2;       // Oko smoka — smoki z Portalu i labiryntu ×2
    public const int Pelnia = 3;         // Pełnia — najsilniejsza magia (Lustro/Ściany)
    public const int GarbAutora = 4;     // Garb Autora — łupy z labiryntu −50%
    public const int PeknietaTarcza = 5; // Pęknięta tarcza — Renowacja/Ambulatorium osłabione

    public const string PhaseKey = "MoonPhase";
    public const string BloodMoonKey = "MoonBloodMoon";

    /// <summary>
    /// Szansa, że w danym cyklu Pełnię zastąpi Krwawy Księżyc (źródło mówi tylko
    /// „co jakiś czas, losowo" — wartość do kalibracji).
    /// </summary>
    private const double BloodMoonChance = 0.25;

    public static readonly string[] PhaseNames =
    {
        "Nów", "Złoty sierp", "Oko smoka", "Pełnia", "Garb Autora", "Pęknięta tarcza"
    };

    public static string DisplayName(int phase, bool bloodMoon) =>
        bloodMoon && phase == Pelnia ? "Krwawy Księżyc" : PhaseNames[((phase % 6) + 6) % 6];

    /// <summary>Przesuwa cykl o jedną fazę — wywoływane raz, na początku przeliczenia.</summary>
    public static async Task<(int Phase, bool BloodMoon)> AdvanceAsync(ApplicationDbContext context)
    {
        var (phase, _) = await GetAsync(context);
        int next = (phase + 1) % 6;
        bool blood = next == Pelnia && Random.Shared.NextDouble() < BloodMoonChance;
        await SetAsync(context, PhaseKey, next.ToString());
        await SetAsync(context, BloodMoonKey, blood ? "1" : "0");
        await context.SaveChangesAsync();
        return (next, blood);
    }

    /// <summary>Aktualna faza i flaga Krwawego Księżyca.</summary>
    public static async Task<(int Phase, bool BloodMoon)> GetAsync(ApplicationDbContext context)
    {
        var settings = await context.GameSettings
            .Where(s => s.Key == PhaseKey || s.Key == BloodMoonKey)
            .ToDictionaryAsync(s => s.Key, s => s.Value);
        int phase = settings.TryGetValue(PhaseKey, out var p) && int.TryParse(p, out var pv)
            ? ((pv % 6) + 6) % 6 : 0;
        bool blood = settings.TryGetValue(BloodMoonKey, out var b) && b == "1";
        return (phase, blood);
    }

    private static async Task SetAsync(ApplicationDbContext context, string key, string value)
    {
        var setting = await context.GameSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) context.GameSettings.Add(new GameSetting { Key = key, Value = value });
        else setting.Value = value;
    }

    /// <summary>
    /// Czy faza działa na księstwo danej rasy. Br-Oug podlega tylko Nowiowi
    /// i Krwawemu Księżycowi.
    /// </summary>
    public static bool Affects(string race, int phase, bool bloodMoon) =>
        race != "Br-Oug" || phase == Now || (bloodMoon && phase == Pelnia);
}
