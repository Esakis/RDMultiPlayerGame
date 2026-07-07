using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface ILabyrinthService
{
    Task<LabyrinthStatusDto> GetStatusAsync(int userId);
    Task<ServiceResult<LabyrinthStatusDto>> TakeTreasureAsync(int userId, int generalId, string treasureType);
    Task<ServiceResult<LabyrinthStatusDto>> SearchGeneralAsync(int userId, int generalId);
    Task<ServiceResult<LabyrinthStatusDto>> ChangeAbilityAsync(int userId, int generalId);
}

/// <summary>
/// Labirynt wg oryginału Red Dragon (docs/MECHANIKA.md §13, docs/zrodla/manual-pl/labyrint.txt).
/// Każde przeliczenie daje budżet akcji (2 pkt, 4 z Sanktuarium Stwórcy). Akcje:
///   • Weź skarb (2 pkt) — jeden z 7 typów łupu; możliwe zranienie/śmierć generała
///     albo klątwa (oprócz złota, które jest zawsze bezpieczne). Skarb wynosisz dopiero
///     po 5. turze danego przeliczenia.
///   • Szukaj generała / Zmień zdolność na Ołtarzu (po 1 pkt) — nigdy nie ranią ani nie zabijają.
/// Im wyższy poziom generała i silniejsze zaklęcie „Szczęście" (Fortuna), tym większy łup
/// i mniejsze ryzyko. Generałowie powyżej 20. poziomu nie giną — z wyjątkiem brania
/// Doświadczenia. Akcje nie kosztują tur.
/// </summary>
public class LabyrinthService : ILabyrinthService
{
    private readonly ApplicationDbContext _context;
    private readonly IGeneralService _generalService;

    private const int TreasureCost = 2;
    private const int GeneralActionCost = 1;
    private const int TurnsRequiredForTreasure = 5;

    /// <summary>Katalog skarbów (Type, Name, Description, RiskyForGeneral).</summary>
    public static readonly (string Type, string Name, string Description, bool Risky)[] TreasureCatalog =
    {
        ("Zloto", "Sakwa złota", "Złoto skalowane obszarem i poziomem generała — zawsze się udaje, bez ryzyka.", false),
        ("Surowce", "Skrzynia surowców", "Losowy surowiec: kamień, jedzenie, broń albo mana.", true),
        ("Doswiadczenie", "Eliksir doświadczenia", "~20% doświadczenia do awansu dla wysłanego generała (ryzyko śmierci niezależne od poziomu).", true),
        ("Portal", "Portal smoczy", "Otwiera portal — do 5 smoków (mniej, gdy masz ich już dużo).", true),
        ("Infrastruktura", "Budulec", "Materiał budowlany — 1/6 wartości znalezionego kamienia.", true),
        ("Nauka", "Kryształ wiedzy", "Punkty nauki w ilości obszar × 5.", true),
        ("Ludnosc", "Pielgrzymi", "Nowi mieszkańcy (obszar/12 – obszar/6); może przekroczyć limit zaludnienia.", true)
    };

    public LabyrinthService(ApplicationDbContext context, IGeneralService generalService)
    {
        _context = context;
        _generalService = generalService;
    }

    private async Task<Kingdom?> GetKingdomAsync(int userId) =>
        await _context.Kingdoms
            .Include(k => k.Buildings)
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive && k.User.ActiveKingdomId == k.Id && !k.IsSuspended);

    /// <summary>
    /// Zajazd u Czerwonego Smoka pozwala wejść do labiryntu dwa razy na przeliczenie
    /// (podwaja budżet akcji). Bez niego można wejść tylko raz.
    /// </summary>
    private static bool HasDoubleEntry(Kingdom k) =>
        k.Buildings.Any(b => b.BuildingType == "ZajazdCzerwonego" && b.Quantity > 0 && !b.IsUnderConstruction);

    private static int MaxActionPoints(Kingdom k) => HasDoubleEntry(k) ? 4 : 2;

    private static int TurnsUsed(Kingdom k) => Math.Max(0, k.TurnsCapacity - k.TurnsAvailable);

    /// <summary>
    /// Fortuna efektywna: zaklęcie „Szczęście" + poziom generała-Odkrywcy (docs/zrodla manual:
    /// Explorer dodaje swój poziom do wpływu Fortuny — większy łup, mniejsze ryzyko). Cap 80.
    /// </summary>
    private static int EffectiveFortune(int baseFortune, General general)
    {
        int explorer = general.SecondaryTrait == "Odkrywca" ? general.Level : 0;
        return Math.Min(80, baseFortune + explorer);
    }

    private async Task<int> DragonLoreAsync(int kingdomId) =>
        await _context.Researches.CountAsync(r =>
            r.KingdomId == kingdomId && r.IsCompleted && r.TechType.StartsWith("Smoko"));

    /// <summary>Siła zaklęcia „Szczęście" (fart w labiryncie), w procentach, max 49.</summary>
    private async Task<int> FortuneAsync(int kingdomId)
    {
        int power = await _context.ActiveSpells
            .Where(s => s.KingdomId == kingdomId && s.SpellType == "Szczescie"
                        && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .Select(s => (int?)s.Power)
            .MaxAsync() ?? 0;
        return Math.Clamp(power, 0, 49);
    }

    public async Task<LabyrinthStatusDto> GetStatusAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null) return new LabyrinthStatusDto();
        return await BuildStatusAsync(kingdom, null);
    }

    private async Task<LabyrinthStatusDto> BuildStatusAsync(Kingdom kingdom, string? lastEvent)
    {
        var generals = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id && !g.IsOutside && !g.IsImprisoned && !g.IsPending
                        && (g.WoundedUntil == null || g.WoundedUntil <= DateTime.UtcNow))
            .OrderByDescending(g => g.Experience)
            .ToListAsync();

        int max = MaxActionPoints(kingdom);
        int remaining = Math.Max(0, max - kingdom.LabyrinthActionsUsed);
        int turnsUsed = TurnsUsed(kingdom);

        return new LabyrinthStatusDto
        {
            ActionPoints = remaining,
            MaxActionPoints = max,
            TreasureCost = TreasureCost,
            GeneralActionCost = GeneralActionCost,
            HasDoubleEntry = HasDoubleEntry(kingdom),
            TurnsUsedThisRecount = turnsUsed,
            TurnsRequiredForTreasure = TurnsRequiredForTreasure,
            CanTakeTreasure = remaining >= TreasureCost && turnsUsed >= TurnsRequiredForTreasure,
            FortuneLevel = await FortuneAsync(kingdom.Id),
            AvailableGenerals = generals.Select(g => new LabyrinthGeneralDto
            {
                Id = g.Id,
                Name = g.Name,
                Level = g.Level,
                PrimaryTrait = g.PrimaryTrait,
                SecondaryTrait = g.SecondaryTrait
            }).ToList(),
            Treasures = TreasureCatalog.Select(t => new LabyrinthTreasureDto
            {
                Type = t.Type, Name = t.Name, Description = t.Description, RiskyForGeneral = t.Risky
            }).ToList(),
            LastEvent = lastEvent
        };
    }

    // ---- Walidacja wspólna -------------------------------------------------

    private async Task<(Kingdom? kingdom, General? general, string? error)> LoadForActionAsync(int userId, int generalId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null) return (null, null, "Nie znaleziono księstwa.");
        if (kingdom.IsFrozen) return (null, null, "Księstwo jest zamrożone — odmróź je, aby działać.");

        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == generalId && g.KingdomId == kingdom.Id);
        if (general == null) return (kingdom, null, "Nie znaleziono generała.");
        if (general.IsPending) return (kingdom, null, "Ten generał czeka jeszcze na decyzję.");
        if (general.IsOutside) return (kingdom, null, "Generał jest poza księstwem.");
        if (general.IsImprisoned) return (kingdom, null, "Generał jest uwięziony.");
        if (general.WoundedUntil.HasValue && general.WoundedUntil > DateTime.UtcNow)
            return (kingdom, null, "Generał jest ranny.");

        return (kingdom, general, null);
    }

    // ---- Branie skarbu -----------------------------------------------------

    public async Task<ServiceResult<LabyrinthStatusDto>> TakeTreasureAsync(int userId, int generalId, string treasureType)
    {
        var (kingdom, general, error) = await LoadForActionAsync(userId, generalId);
        if (error != null) return ServiceResult<LabyrinthStatusDto>.Fail(error);

        var treasure = TreasureCatalog.FirstOrDefault(t => t.Type == treasureType);
        if (treasure.Type == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nieznany typ skarbu.");

        int remaining = MaxActionPoints(kingdom!) - kingdom!.LabyrinthActionsUsed;
        if (remaining < TreasureCost)
            return ServiceResult<LabyrinthStatusDto>.Fail($"Za mało punktów akcji (potrzeba {TreasureCost}).");
        if (TurnsUsed(kingdom) < TurnsRequiredForTreasure)
            return ServiceResult<LabyrinthStatusDto>.Fail($"Skarb można wynieść dopiero po {TurnsRequiredForTreasure}. turze tego przeliczenia.");

        kingdom.LabyrinthActionsUsed += TreasureCost;

        int lvl = general!.Level;
        int fortune = EffectiveFortune(await FortuneAsync(kingdom.Id), general);
        var rng = Random.Shared;

        // Złoto jest zawsze bezpieczne; pozostałe skarby niosą ryzyko.
        if (treasure.Risky)
        {
            double luck = fortune / 100.0;
            double levelFactor = Math.Min(0.85, lvl * 0.03);
            double badChance = 0.30 * (1 - luck) * (1 - levelFactor);

            if (rng.NextDouble() < badChance)
            {
                string bad = ResolveBadEvent(kingdom, general, lvl, treasureType == "Doswiadczenie", rng);
                await _context.SaveChangesAsync();
                var st = await BuildStatusAsync(kingdom, bad);
                return ServiceResult<LabyrinthStatusDto>.Ok(st, bad);
            }
        }

        string message = await ApplyTreasureAsync(kingdom, general, treasureType, lvl, fortune, rng);
        await _context.SaveChangesAsync();

        var status = await BuildStatusAsync(kingdom, message);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }

    /// <summary>Złe zdarzenie przy braniu skarbu: rana, klątwa lub śmierć generała.</summary>
    private string ResolveBadEvent(Kingdom kingdom, General general, int lvl, bool experienceTreasure, Random rng)
    {
        // Generałowie powyżej 20. poziomu nie giną — chyba że brali Doświadczenie.
        bool canDie = lvl <= 20 || experienceTreasure;
        int roll = rng.Next(100);

        if (roll < 45)
        {
            // Pałac (Dracopedia §14.3): ranni generałowie wracają do sił 2× szybciej
            bool hasPalac = kingdom.Buildings != null && kingdom.Buildings.Any(b =>
                b.BuildingType == "Palac" && b.Quantity > 0 && !b.IsUnderConstruction);
            general.WoundedUntil = DateTime.UtcNow.AddHours(hasPalac ? 6 : 12);
            return $"{general.Name} wpadł w pułapkę i wraca ranny z pustymi rękami.";
        }
        if (roll < 70 || !canDie)
        {
            // Klątwa rzucona na księstwo (negatywne zdarzenie zamiast pełnego zaklęcia)
            int popLoss = rng.Next(5, 11);
            kingdom.Popularity = Math.Max(0, kingdom.Popularity - popLoss);
            return $"Strażnik labiryntu rzucił klątwę na księstwo — popularność spada o {popLoss}. Skarb przepadł.";
        }

        // Śmierć generała
        _context.Generals.Remove(general);
        return $"{general.Name} zginął w mroku labiryntu. Skarb przepadł wraz z nim.";
    }

    /// <summary>Nakłada wybrany skarb na księstwo i zwraca komunikat.</summary>
    private async Task<string> ApplyTreasureAsync(Kingdom kingdom, General general, string type, int lvl, int fortune, Random rng)
    {
        decimal luckMult = 1m + fortune / 100m;
        // Elf (cecha rasowa, MECHANIKA §2.2): 1,5× łupy z labiryntu
        if (kingdom.Race == "Elf") luckMult *= 1.5m;
        double rand = 0.8 + rng.NextDouble() * 0.4; // 0,8–1,2

        switch (type)
        {
            case "Zloto":
            {
                long gold = (long)(kingdom.Land * (40 + lvl * 8) * luckMult * (decimal)rand);
                kingdom.Gold += gold;
                return $"{general.Name} wyniósł z labiryntu {gold:N0} złota.";
            }
            case "Surowce":
            {
                long amount = (long)(kingdom.Land * (20 + lvl * 5) * luckMult * (decimal)rand);
                switch (rng.Next(4))
                {
                    case 0: kingdom.Stone += amount; return $"{general.Name} odnalazł {amount:N0} kamienia.";
                    case 1: kingdom.Food += amount; return $"{general.Name} odnalazł {amount:N0} jedzenia.";
                    case 2: kingdom.Weapons += amount; return $"{general.Name} odnalazł {amount:N0} broni.";
                    default: kingdom.Mana += amount; return $"{general.Name} odnalazł {amount:N0} many.";
                }
            }
            case "Doswiadczenie":
            {
                long gain = Math.Max(1, (long)(general.ExperienceToNextLevel * 0.2 * rand));
                general.Experience += gain;
                return $"{general.Name} zdobył {gain:N0} doświadczenia.";
            }
            case "Portal":
                return await ApplyPortalAsync(kingdom);
            case "Infrastruktura":
            {
                long stoneEquiv = (long)(kingdom.Land * (20 + lvl * 5) * luckMult * (decimal)rand);
                long budulec = stoneEquiv / 6;
                kingdom.BudulecStored += budulec;
                return $"{general.Name} wyniósł {budulec:N0} budulca.";
            }
            case "Nauka":
            {
                long science = kingdom.Land * 5L;
                kingdom.SciencePoints += science;
                return $"{general.Name} odnalazł kryształ wiedzy: +{science:N0} punktów nauki.";
            }
            case "Ludnosc":
            {
                int people = rng.Next(Math.Max(1, kingdom.Land / 12), Math.Max(2, kingdom.Land / 6) + 1);
                kingdom.Population += people;
                return $"Do księstwa przybyło {people:N0} pielgrzymów (mieszkańców).";
            }
            default:
                return "Pusta komnata — nic nie znaleziono.";
        }
    }

    /// <summary>Portal smoczy — do 5 smoków, mniej gdy masz ich już dużo; respektuje limit.</summary>
    private async Task<string> ApplyPortalAsync(Kingdom kingdom)
    {
        long dragons = await _context.MilitaryUnits
            .Where(m => m.KingdomId == kingdom.Id && m.UnitType.EndsWith("_Smok"))
            .SumAsync(m => (long)m.Quantity);

        int draco = await DragonLoreAsync(kingdom.Id);
        long cap = DragonHelper.ComputeCap(kingdom, draco);

        // Maks. z labiryntu: 5 do 150 smoków, liniowo do 1 przy 240+.
        int maxFromLab = dragons >= 240 ? 1
            : dragons >= 150 ? Math.Max(1, 5 - (int)((dragons - 150) * 4 / 90))
            : 5;

        long canAdd = Math.Min(maxFromLab, cap - dragons);
        if (canAdd <= 0)
            return "Portal się otworzył, ale osiągnąłeś już limit smoków — żaden nie przeszedł.";

        var dragonDef = await _context.UnitDefinitions
            .FirstOrDefaultAsync(u => u.Race == kingdom.Race && u.UnitType.EndsWith("_Smok"));
        if (dragonDef == null)
            return "Portal się otworzył, lecz żaden smok nie nadszedł.";

        var unit = await _context.MilitaryUnits
            .FirstOrDefaultAsync(m => m.KingdomId == kingdom.Id && m.UnitType == dragonDef.UnitType);
        if (unit == null)
            _context.MilitaryUnits.Add(new MilitaryUnit
            {
                KingdomId = kingdom.Id, UnitType = dragonDef.UnitType, Quantity = (int)canAdd
            });
        else
            unit.Quantity += (int)canAdd;

        return $"Przez portal przeszło {canAdd} smoków!";
    }

    // ---- Akcje generała (po 1 pkt, bez ryzyka) -----------------------------

    public async Task<ServiceResult<LabyrinthStatusDto>> SearchGeneralAsync(int userId, int generalId)
    {
        var (kingdom, general, error) = await LoadForActionAsync(userId, generalId);
        if (error != null) return ServiceResult<LabyrinthStatusDto>.Fail(error);

        int remaining = MaxActionPoints(kingdom!) - kingdom!.LabyrinthActionsUsed;
        if (remaining < GeneralActionCost)
            return ServiceResult<LabyrinthStatusDto>.Fail("Za mało punktów akcji.");

        kingdom.LabyrinthActionsUsed += GeneralActionCost;

        int fortune = EffectiveFortune(await FortuneAsync(kingdom.Id), general!);
        bool found = await _generalService.TryLabyrinthFindGeneralAsync(kingdom, general!.Level, fortune);

        string message = found
            ? $"{general.Name} odnalazł w labiryncie nowego generała — czeka na Twoją decyzję."
            : $"{general.Name} przeszukał korytarze, ale nie znalazł żadnego generała.";

        await _context.SaveChangesAsync();
        var status = await BuildStatusAsync(kingdom, message);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> ChangeAbilityAsync(int userId, int generalId)
    {
        var (kingdom, general, error) = await LoadForActionAsync(userId, generalId);
        if (error != null) return ServiceResult<LabyrinthStatusDto>.Fail(error);

        int remaining = MaxActionPoints(kingdom!) - kingdom!.LabyrinthActionsUsed;
        if (remaining < GeneralActionCost)
            return ServiceResult<LabyrinthStatusDto>.Fail("Za mało punktów akcji.");

        kingdom.LabyrinthActionsUsed += GeneralActionCost;

        string newTrait = _generalService.ChangeSecondaryFromAltar(general!);
        string message = $"Ołtarz przemienił {general!.Name}: nowa cecha drugorzędna to {newTrait} (kosztem połowy doświadczenia).";

        await _context.SaveChangesAsync();
        var status = await BuildStatusAsync(kingdom, message);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }
}
