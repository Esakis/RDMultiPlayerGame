using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface ILabyrinthService
{
    Task<LabyrinthStatusDto> GetStatusAsync(int userId);
    Task<ServiceResult<LabyrinthStatusDto>> EnterAsync(int userId, int generalId);
    Task<ServiceResult<LabyrinthStatusDto>> AdvanceAsync(int userId);
    Task<ServiceResult<LabyrinthStatusDto>> RetreatAsync(int userId);
    Task<ServiceResult<LabyrinthStatusDto>> SpendDiceAsync(int userId, string rewardType);
}

/// <summary>
/// Labirynt (docs/MECHANIKA.md §13) — minigra „push your luck".
/// Generał schodzi głębiej (1 tura/poziom), gromadząc łup; każdy poziom niesie ryzyko
/// pułapki (rana, koniec wyprawy — łup zachowany) lub potwora (test przeżycia: porażka =
/// śmierć generała i utrata łupu). Gracz może w każdej chwili się wycofać i zdeponować łup.
/// Elf zbiera 1,5× łupów materialnych.
/// </summary>
public class LabyrinthService : ILabyrinthService
{
    private readonly ApplicationDbContext _context;

    /// <summary>Nagrody do kupienia za zebrane kości (docs/MECHANIKA.md §13).</summary>
    public static readonly (string Type, string Name, string Description, int DiceCost)[] RewardCatalog =
    {
        ("Zloto", "Sakwa złota", "Złoto skalowane obszarem (akry × 200)", 5),
        ("Surowce", "Skrzynia surowców", "Kamień, jedzenie i broń (skalowane obszarem)", 5),
        ("Mana", "Kryształ many", "Mana (akry × 25)", 3),
        ("Doswiadczenie", "Eliksir doświadczenia", "+50 000 doświadczenia najlepszemu generałowi", 8),
        ("Tura", "Klepsydra", "+1 tura (do limitu)", 6)
    };

    public LabyrinthService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Poziom wiedzy o smokach — zwiększa łupy i kości w labiryncie.</summary>
    private async Task<int> DragonLoreAsync(int kingdomId) =>
        await _context.Researches.CountAsync(r =>
            r.KingdomId == kingdomId && r.IsCompleted && r.TechType.StartsWith("Smoko"));

    private async Task<Kingdom?> GetKingdomAsync(int userId) =>
        await _context.Kingdoms.FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

    private async Task<LabyrinthExpedition?> GetActiveExpeditionAsync(int kingdomId) =>
        await _context.LabyrinthExpeditions
            .Include(e => e.General)
            .FirstOrDefaultAsync(e => e.KingdomId == kingdomId && e.Status == "Active");

    public async Task<LabyrinthStatusDto> GetStatusAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return new LabyrinthStatusDto();

        return await BuildStatusAsync(kingdom);
    }

    private async Task<LabyrinthStatusDto> BuildStatusAsync(Kingdom kingdom)
    {
        var active = await GetActiveExpeditionAsync(kingdom.Id);

        var available = await _context.Generals
            .Where(g => g.KingdomId == kingdom.Id && !g.IsOutside && !g.IsImprisoned
                        && (g.WoundedUntil == null || g.WoundedUntil <= DateTime.UtcNow))
            .OrderByDescending(g => g.Experience)
            .ToListAsync();

        return new LabyrinthStatusDto
        {
            HasActiveExpedition = active != null,
            Expedition = active == null ? null : new LabyrinthExpeditionDto
            {
                GeneralId = active.GeneralId ?? 0,
                GeneralName = active.General!.Name,
                GeneralLevel = active.General!.Level,
                Depth = active.Depth,
                PendingGold = active.PendingGold,
                PendingFood = active.PendingFood,
                PendingStone = active.PendingStone,
                PendingWeapons = active.PendingWeapons,
                PendingMana = active.PendingMana,
                PendingDice = active.PendingDice,
                LastEvent = active.LastEvent
            },
            AvailableGenerals = available.Select(g => new LabyrinthGeneralDto
            {
                Id = g.Id,
                Name = g.Name,
                Level = g.Level,
                PrimaryTrait = g.PrimaryTrait
            }).ToList(),
            BankedDice = kingdom.LabyrinthDice,
            TurnsAvailable = kingdom.TurnsAvailable,
            DragonLore = await DragonLoreAsync(kingdom.Id),
            Rewards = RewardCatalog.Select(r => new LabyrinthRewardDto
            {
                Type = r.Type,
                Name = r.Name,
                Description = r.Description,
                DiceCost = r.DiceCost,
                CanAfford = kingdom.LabyrinthDice >= r.DiceCost
            }).ToList()
        };
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> EnterAsync(int userId, int generalId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono księstwa.");
        if (kingdom.IsFrozen)
            return ServiceResult<LabyrinthStatusDto>.Fail("Księstwo jest zamrożone — odmróź je, aby działać.");

        if (await GetActiveExpeditionAsync(kingdom.Id) != null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Inny generał jest już w labiryncie.");

        var general = await _context.Generals
            .FirstOrDefaultAsync(g => g.Id == generalId && g.KingdomId == kingdom.Id);
        if (general == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono generała.");
        if (general.IsOutside)
            return ServiceResult<LabyrinthStatusDto>.Fail("Generał jest poza księstwem.");
        if (general.IsImprisoned)
            return ServiceResult<LabyrinthStatusDto>.Fail("Generał jest uwięziony.");
        if (general.WoundedUntil.HasValue && general.WoundedUntil > DateTime.UtcNow)
            return ServiceResult<LabyrinthStatusDto>.Fail("Generał jest ranny.");

        general.IsOutside = true; // zajęty w labiryncie

        _context.LabyrinthExpeditions.Add(new LabyrinthExpedition
        {
            KingdomId = kingdom.Id,
            GeneralId = general.Id,
            Depth = 0,
            Status = "Active",
            LastEvent = $"{general.Name} wkracza do labiryntu. Mrok pochłania światło pochodni..."
        });
        await _context.SaveChangesAsync();

        var status = await BuildStatusAsync(kingdom);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, $"{general.Name} wkroczył do labiryntu.");
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> AdvanceAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono księstwa.");
        if (kingdom.IsFrozen)
            return ServiceResult<LabyrinthStatusDto>.Fail("Księstwo jest zamrożone — odmróź je, aby działać.");
        if (kingdom.TurnsAvailable <= 0)
            return ServiceResult<LabyrinthStatusDto>.Fail("Brak dostępnych tur.");

        var exp = await GetActiveExpeditionAsync(kingdom.Id);
        if (exp == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Żaden generał nie jest w labiryncie.");

        kingdom.TurnsAvailable--;
        exp.Depth++;
        int d = exp.Depth;
        var rng = Random.Shared;
        bool elf = kingdom.Race == "Elf";
        int dragonLore = await DragonLoreAsync(kingdom.Id);
        // Elf zbiera 1,5×; wiedza o smokach +15% łupów/kości za poziom (manual)
        decimal lootMult = (elf ? 1.5m : 1.0m) * (1m + dragonLore * 0.15m);
        var general = exp.General!;

        // Łup materialny skalowany głębią
        long Loot(int lo, int hi) => (long)(d * rng.Next(lo, hi) * lootMult);

        // Szanse rosną z głębią
        int monsterChance = Math.Min(35, 8 + d);
        int trapChance = Math.Min(30, 12 + d / 2);
        int diceChance = 12;
        int emptyChance = 10;
        int roll = rng.Next(100);

        string message;

        if (roll < monsterChance)
        {
            // Potwór — test przeżycia (zależny od poziomu i głębi)
            int lvl = general.Level;
            double survive = (double)(lvl * 8) / (lvl * 8 + d);
            if (rng.NextDouble() <= survive)
            {
                // Przeżył — pokonany potwór strzeże skarbu (podwójny łup), wyprawa trwa
                exp.PendingGold += Loot(120, 240);
                exp.PendingWeapons += Loot(60, 140);
                exp.PendingDice += 1 + dragonLore + (rng.Next(2) == 0 ? 1 : 0);
                general.Experience += d * 90;
                message = $"Poziom {d}: {general.Name} pokonał strażnika labiryntu i splądrował jego skarbiec!";
                exp.LastEvent = message;
            }
            else
            {
                // Zginął — łup przepada, generał usunięty
                message = $"Poziom {d}: {general.Name} zginął w starciu ze strażnikiem labiryntu. Łup przepadł.";
                exp.GeneralId = null;
                exp.General = null;
                exp.Status = "Ended";
                _context.Generals.Remove(general);
                exp.LastEvent = message;
                await _context.SaveChangesAsync();
                var diedStatus = await BuildStatusAsync(kingdom);
                return ServiceResult<LabyrinthStatusDto>.Ok(diedStatus, message);
            }
        }
        else if (roll < monsterChance + trapChance)
        {
            // Pułapka — generał ranny, wyprawa kończy się, łup zdeponowany
            general.WoundedUntil = DateTime.UtcNow.AddHours(12);
            general.IsOutside = false;
            general.Experience += d * 30;
            BankLoot(kingdom, exp);
            message = $"Poziom {d}: {general.Name} wpadł w pułapkę i wraca ranny, ale z łupem.";
            exp.Status = "Ended";
            exp.LastEvent = message;
            await _context.SaveChangesAsync();
            var trapStatus = await BuildStatusAsync(kingdom);
            return ServiceResult<LabyrinthStatusDto>.Ok(trapStatus, message);
        }
        else if (roll < monsterChance + trapChance + diceChance)
        {
            int dice = rng.Next(1, 4) + dragonLore;
            exp.PendingDice += dice;
            general.Experience += d * 20;
            message = $"Poziom {d}: {general.Name} znalazł {dice} magicznych kości.";
            exp.LastEvent = message;
        }
        else if (roll < monsterChance + trapChance + diceChance + emptyChance)
        {
            general.Experience += d * 10;
            message = $"Poziom {d}: pusta, zakurzona komnata. Nic ciekawego.";
            exp.LastEvent = message;
        }
        else if (rng.Next(7) == 0)
        {
            // Bogata komnata skarbów (jackpot) — duży łup i garść kości
            exp.PendingGold += Loot(300, 600);
            exp.PendingWeapons += Loot(80, 200);
            int dice = 2 + dragonLore + rng.Next(3);
            exp.PendingDice += dice;
            general.Experience += d * 70;
            message = $"Poziom {d}: {general.Name} natrafił na bogatą komnatę skarbów! ({dice} kości)";
            exp.LastEvent = message;
        }
        else
        {
            // Skarb — losowy zestaw surowców
            exp.PendingGold += Loot(80, 160);
            switch (rng.Next(4))
            {
                case 0: exp.PendingStone += Loot(40, 100); break;
                case 1: exp.PendingFood += Loot(40, 100); break;
                case 2: exp.PendingWeapons += Loot(30, 80); break;
                default: exp.PendingMana += Loot(20, 60); break;
            }
            general.Experience += d * 50;
            message = $"Poziom {d}: {general.Name} odnalazł skrytkę ze skarbem.";
            exp.LastEvent = message;
        }

        await _context.SaveChangesAsync();
        var status = await BuildStatusAsync(kingdom);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> RetreatAsync(int userId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono księstwa.");

        var exp = await GetActiveExpeditionAsync(kingdom.Id);
        if (exp == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Żaden generał nie jest w labiryncie.");

        BankLoot(kingdom, exp);
        exp.General!.IsOutside = false;
        exp.Status = "Ended";
        string message = $"{exp.General.Name} wycofał się z labiryntu (głębokość {exp.Depth}) i zdeponował łup.";
        exp.LastEvent = message;
        await _context.SaveChangesAsync();

        var status = await BuildStatusAsync(kingdom);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> SpendDiceAsync(int userId, string rewardType)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono księstwa.");

        var reward = RewardCatalog.FirstOrDefault(r => r.Type == rewardType);
        if (reward.Type == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nieznana nagroda.");
        if (kingdom.LabyrinthDice < reward.DiceCost)
            return ServiceResult<LabyrinthStatusDto>.Fail($"Za mało kości (potrzeba {reward.DiceCost}).");

        string message;
        switch (rewardType)
        {
            case "Zloto":
                long gold = kingdom.Land * 200L;
                kingdom.Gold += gold;
                message = $"Wymieniono kości na {gold:N0} złota.";
                break;
            case "Surowce":
                long stone = kingdom.Land * 60L, food = kingdom.Land * 60L, weapons = kingdom.Land * 40L;
                kingdom.Stone += stone; kingdom.Food += food; kingdom.Weapons += weapons;
                message = $"Wymieniono kości na surowce ({stone:N0} kamienia, {food:N0} jedzenia, {weapons:N0} broni).";
                break;
            case "Mana":
                long mana = kingdom.Land * 25L;
                kingdom.Mana += mana;
                message = $"Wymieniono kości na {mana:N0} many.";
                break;
            case "Doswiadczenie":
                var gen = await _context.Generals
                    .Where(g => g.KingdomId == kingdom.Id)
                    .OrderByDescending(g => g.Experience)
                    .FirstOrDefaultAsync();
                if (gen == null)
                    return ServiceResult<LabyrinthStatusDto>.Fail("Nie masz generała, który mógłby zdobyć doświadczenie.");
                gen.Experience += 50_000;
                message = $"Generał {gen.Name} zdobył 50 000 doświadczenia.";
                break;
            case "Tura":
                if (kingdom.TurnsAvailable >= kingdom.MaxTurns)
                    return ServiceResult<LabyrinthStatusDto>.Fail("Masz już maksymalną liczbę tur.");
                kingdom.TurnsAvailable = Math.Min(kingdom.MaxTurns, kingdom.TurnsAvailable + 1);
                // Bonusowa tura zwiększa przydział cyklu, by licznik 0→max nie zszedł poniżej zera
                kingdom.TurnsCapacity = Math.Max(kingdom.TurnsCapacity, kingdom.TurnsAvailable);
                message = "Klepsydra dodała 1 turę.";
                break;
            default:
                return ServiceResult<LabyrinthStatusDto>.Fail("Nieznana nagroda.");
        }

        kingdom.LabyrinthDice -= reward.DiceCost;
        await _context.SaveChangesAsync();

        var status = await BuildStatusAsync(kingdom);
        return ServiceResult<LabyrinthStatusDto>.Ok(status, message);
    }

    /// <summary>Przenosi zgromadzony łup wyprawy do skarbca księstwa.</summary>
    private static void BankLoot(Kingdom kingdom, LabyrinthExpedition exp)
    {
        kingdom.Gold += exp.PendingGold;
        kingdom.Food += exp.PendingFood;
        kingdom.Stone += exp.PendingStone;
        kingdom.Weapons += exp.PendingWeapons;
        kingdom.Mana += exp.PendingMana;
        kingdom.LabyrinthDice += exp.PendingDice;
    }
}
