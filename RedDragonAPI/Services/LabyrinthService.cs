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

    public LabyrinthService(ApplicationDbContext context)
    {
        _context = context;
    }

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
            TurnsAvailable = kingdom.TurnsAvailable
        };
    }

    public async Task<ServiceResult<LabyrinthStatusDto>> EnterAsync(int userId, int generalId)
    {
        var kingdom = await GetKingdomAsync(userId);
        if (kingdom == null)
            return ServiceResult<LabyrinthStatusDto>.Fail("Nie znaleziono księstwa.");

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
        decimal lootMult = elf ? 1.5m : 1.0m;
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
                if (rng.Next(2) == 0) exp.PendingDice += 1;
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
            int dice = rng.Next(1, 4);
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
