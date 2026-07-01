using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Helpers;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoalitionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoalitionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<CoalitionDto>>> GetCoalitions([FromQuery] int? eraId)
    {
        int era = eraId ?? 1;
        var coalitions = await _context.Coalitions
            .Where(c => c.EraId == era)
            .Include(c => c.Members)
            .Include(c => c.Leader)
            .ToListAsync();

        // Pełne dane każdego członka — potrzebne do sił bojowych, many i zabudowy.
        var memberIds = coalitions.SelectMany(c => c.Members.Select(m => m.Id)).ToHashSet();
        var fullKingdoms = await _context.Kingdoms
            .Where(k => memberIds.Contains(k.Id))
            .Include(k => k.MilitaryUnits).ThenInclude(u => u.Definition)
            .Include(k => k.Buildings).ThenInclude(b => b.Definition)
            .Include(k => k.Professions)
            .ToDictionaryAsync(k => k.Id);

        var races = await _context.RaceDefinitions.ToDictionaryAsync(r => r.Name);

        var result = coalitions.Select(c =>
        {
            var members = c.Members
                .Select(m => BuildMemberSummary(fullKingdoms.GetValueOrDefault(m.Id) ?? m, races))
                .ToList();
            return new CoalitionDto
            {
                Id = c.Id,
                Name = c.Name,
                Tag = c.Tag,
                LeaderKingdomId = c.LeaderKingdomId,
                LeaderName = c.Leader != null ? c.Leader.Name : null,
                MemberCount = c.Members.Count,
                MaxMembers = c.MaxMembers,
                PSOProgress = c.PSOProgress,
                TotalLand = members.Sum(m => (long)m.Land),
                Members = members
            };
        })
        // Wykaz wszystkich koalicji posortowany malejąco po łącznym obszarze ziemi.
        .OrderByDescending(c => c.TotalLand)
        .ThenBy(c => c.Name)
        .ToList();

        return Ok(result);
    }

    /// <summary>Buduje wiersz statystyk członka koalicji (siły wg wzorów BattleCalculator).</summary>
    private static KingdomSummaryDto BuildMemberSummary(Kingdom k, Dictionary<string, RaceDefinition> races)
    {
        races.TryGetValue(k.Race, out var race);

        // Zabudowa: zajęta ziemia = Σ ilość × koszt ziemi z definicji budynku.
        int buildingCount = k.Buildings?.Sum(b => b.Quantity) ?? 0;
        int usedLand = k.Buildings?.Sum(b => b.Quantity * (b.Definition?.CostLand ?? 0)) ?? 0;
        int freeLand = Math.Max(0, k.Land - usedLand);
        decimal builtPercent = k.Land > 0 ? Math.Round((decimal)usedLand / k.Land * 100m, 1) : 0m;

        // Siła złodziejska: liczba złodziei × (1 + modyfikator skuteczności rasy).
        long thieves = k.MilitaryUnits?.Where(u => u.UnitType.EndsWith("_Zlodziej")).Sum(u => (long)u.Quantity) ?? 0;
        long thiefPower = (long)Math.Round(thieves * (1m + (race?.ThiefPowerModifier ?? 0m)));

        // Siły bojowe — wymagają definicji jednostek/budynków i rasy.
        long attack = 0, defense = 0;
        if (race != null && k.MilitaryUnits != null && k.MilitaryUnits.All(u => u.Definition != null))
        {
            var allUnits = k.MilitaryUnits.ToDictionary(u => u.UnitType, u => u.Quantity);
            attack = BattleCalculator.CalculateAttackPower(k, allUnits, race);
            defense = BattleCalculator.CalculateDefensePower(k, race);
        }

        return new KingdomSummaryDto
        {
            Id = k.Id,
            Name = k.Name,
            Race = k.Race,
            Land = k.Land,
            Population = k.Population,
            Gold = k.Gold,
            Military = k.MilitaryUnits?.Sum(u => u.Quantity + u.InTraining) ?? 0,
            AttackPower = attack,
            DefensePower = defense,
            Magic = k.Mana,
            ThiefPower = thiefPower,
            BuildingCount = buildingCount,
            UsedLand = usedLand,
            FreeLand = freeLand,
            BuiltPercent = builtPercent,
            CoalitionRole = k.CoalitionRole
        };
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateCoalitionDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        if (kingdom.CoalitionId.HasValue)
            return BadRequest("Już należysz do koalicji.");

        var coalition = new Coalition
        {
            Name = dto.Name,
            Tag = dto.Tag,
            LeaderKingdomId = kingdom.Id,
            EraId = kingdom.EraId,
            MaxMembers = 17
        };

        _context.Coalitions.Add(coalition);
        await _context.SaveChangesAsync();

        kingdom.CoalitionId = coalition.Id;
        kingdom.CoalitionRole = "Imperator";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Koalicja '{dto.Name}' została utworzona." });
    }

    [HttpPost("join")]
    public async Task<ActionResult> Join([FromBody] JoinCoalitionDto dto)
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == dto.CoalitionId);

        if (coalition == null)
            return NotFound("Nie znaleziono koalicji.");

        if (kingdom.CoalitionId == coalition.Id)
            return BadRequest("Już należysz do tej koalicji.");

        if (coalition.Members.Count >= coalition.MaxMembers)
            return BadRequest("Koalicja jest pełna.");

        // Swobodne przeskakiwanie: jeśli należysz już do innej koalicji, najpierw ją opuść.
        if (kingdom.CoalitionId.HasValue)
            await DetachFromCoalitionAsync(kingdom);

        kingdom.CoalitionId = coalition.Id;
        kingdom.CoalitionRole = "Member";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Dołączono do koalicji '{coalition.Name}'." });
    }

    [HttpPost("leave")]
    public async Task<ActionResult> Leave()
    {
        var userId = GetUserId();
        var kingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);

        if (kingdom == null)
            return NotFound("Nie znaleziono księstwa.");

        if (!kingdom.CoalitionId.HasValue)
            return BadRequest("Nie należysz do żadnej koalicji.");

        await DetachFromCoalitionAsync(kingdom);
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Opuszczono koalicję." });
    }

    /// <summary>
    /// Odłącza księstwo od jego obecnej koalicji: przekazuje przywództwo lub rozwiązuje
    /// pustą koalicję oraz czyści powiązane głosy wyborcze. Nie wywołuje SaveChanges.
    /// </summary>
    private async Task DetachFromCoalitionAsync(Kingdom kingdom)
    {
        var coalition = await _context.Coalitions
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition != null && coalition.LeaderKingdomId == kingdom.Id)
        {
            // Lider opuszcza - wyznacz nowego lub rozwiąż
            var newLeader = await _context.Kingdoms
                .Where(k => k.CoalitionId == coalition.Id && k.Id != kingdom.Id)
                .FirstOrDefaultAsync();

            if (newLeader != null)
            {
                coalition.LeaderKingdomId = newLeader.Id;
                newLeader.CoalitionRole = "Imperator";
            }
            else
            {
                _context.Coalitions.Remove(coalition);
            }
        }

        // Wyczyść głosy wyborcze związane z odchodzącym księstwem
        var voters = await _context.Kingdoms
            .Where(k => k.ImperatorVoteForKingdomId == kingdom.Id).ToListAsync();
        foreach (var v in voters) v.ImperatorVoteForKingdomId = null;
        kingdom.ImperatorVoteForKingdomId = null;

        kingdom.CoalitionId = null;
        kingdom.CoalitionRole = null;
    }

    [HttpPost("appoint-main-commander")]
    public async Task<ActionResult<ServiceResult>> AppointMainCommander([FromBody] AppointMainCommanderDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może mianować Głównodowodzącego.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        var targetKingdom = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.KingdomId && k.CoalitionId == coalition.Id);

        if (targetKingdom == null) return NotFound("Wybrane księstwo nie należy do tej koalicji.");
        if (targetKingdom.Id == kingdom.Id) return BadRequest("Nie możesz mianować siebie.");

        // Remove existing MainCommander if any
        var existingCommander = coalition.Members.FirstOrDefault(m => m.CoalitionRole == "MainCommander");
        if (existingCommander != null)
        {
            existingCommander.CoalitionRole = "Member";
        }

        // Appoint new MainCommander
        targetKingdom.CoalitionRole = "MainCommander";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Mianowano {targetKingdom.Name} na Głównodowodzącego." });
    }

    [HttpPost("remove-main-commander")]
    public async Task<ActionResult<ServiceResult>> RemoveMainCommander()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do żadnej koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może usunąć Głównodowodzącego.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);

        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        var commander = coalition.Members.FirstOrDefault(m => m.CoalitionRole == "MainCommander");
        if (commander == null) return BadRequest("Nie ma Głównodowodzącego w koalicji.");

        commander.CoalitionRole = "Member";
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Usunięto {commander.Name} z funkcji Głównodowodzącego." });
    }

    // === Pałac Sądu Ostatecznego (PPS) — docs/MECHANIKA.md §12, §14.3 ===
    private const long PpsCost = 10_000_000;        // budulec na ukończenie
    private const long PpsRequiredLand = 750_000;   // min. obszar koalicji przez całą budowę

    [HttpGet("pps")]
    public async Task<ActionResult<PpsStatusDto>> GetPps()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");

        if (kingdom.CoalitionId == null)
            return Ok(new PpsStatusDto
            {
                HasCoalition = false, Cost = PpsCost, RequiredLand = PpsRequiredLand,
                MyBudulecStored = kingdom.BudulecStored
            });

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        long land = coalition.Members.Sum(m => (long)m.Land);
        return Ok(new PpsStatusDto
        {
            HasCoalition = true,
            IsBuilding = coalition.IsBuildingPps,
            InvestedBudulec = coalition.PpsBudulec,
            Cost = PpsCost,
            Percent = Math.Round((decimal)coalition.PpsBudulec / PpsCost * 100m, 2),
            CoalitionLand = land,
            RequiredLand = PpsRequiredLand,
            LandThresholdMet = land >= PpsRequiredLand,
            IsLeader = coalition.LeaderKingdomId == kingdom.Id,
            Role = kingdom.CoalitionRole,
            MyBudulecStored = kingdom.BudulecStored
        });
    }

    [HttpPost("pps/start")]
    public async Task<ActionResult<ServiceResult>> StartPps()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może rozpocząć budowę PPS.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");
        if (coalition.IsBuildingPps) return BadRequest("Budowa PPS już trwa.");

        long land = coalition.Members.Sum(m => (long)m.Land);
        if (land < PpsRequiredLand)
            return BadRequest($"Koalicja musi mieć co najmniej {PpsRequiredLand:N0} akrów (ma {land:N0}).");

        coalition.IsBuildingPps = true;
        await _context.SaveChangesAsync();
        return Ok(new ServiceResult { Success = true, Message = "Rozpoczęto budowę Pałacu Sądu Ostatecznego!" });
    }

    [HttpPost("pps/contribute")]
    public async Task<ActionResult<ServiceResult>> ContributePps([FromBody] ContributePpsDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (dto.Budulec <= 0) return BadRequest("Podaj dodatnią ilość budulca.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");
        if (!coalition.IsBuildingPps) return BadRequest("Koalicja nie rozpoczęła budowy PPS.");

        long give = Math.Min(dto.Budulec, kingdom.BudulecStored);
        if (give <= 0) return BadRequest("Nie masz budulca w magazynie.");

        kingdom.BudulecStored -= give;
        coalition.PpsBudulec += give;
        coalition.PSOProgress = Math.Min(100m, (decimal)coalition.PpsBudulec / PpsCost * 100m);

        long land = coalition.Members.Sum(m => (long)m.Land);

        if (coalition.PpsBudulec >= PpsCost && land >= PpsRequiredLand)
        {
            await _context.SaveChangesAsync();
            await EraConcluder.ConcludeAsync(_context, coalition);
            return Ok(new ServiceResult
            {
                Success = true,
                Message = "Pałac Sądu Ostatecznego ukończony! Era zakończona — Wasza koalicja trafia do Panteonu. Świat odradza się od nowa."
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new ServiceResult
        {
            Success = true,
            Message = $"Wpłacono {give:N0} budulca. Postęp PPS: {coalition.PSOProgress:0.##}% (obszar koalicji {land:N0}/{PpsRequiredLand:N0})."
        });
    }

    // === Kasa koalicji (docs/MECHANIKA.md §12) ===

    [HttpGet("treasury")]
    public async Task<ActionResult<TreasuryDto>> GetTreasury()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null)
            return Ok(new TreasuryDto { HasCoalition = false, MyGold = kingdom.Gold, MyBudulecStored = kingdom.BudulecStored });

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        return Ok(new TreasuryDto
        {
            HasCoalition = true,
            TreasuryGold = coalition.TreasuryGold,
            TreasuryBudulec = coalition.TreasuryBudulec,
            IsLeader = coalition.LeaderKingdomId == kingdom.Id,
            MyGold = kingdom.Gold,
            MyBudulecStored = kingdom.BudulecStored,
            IsBuildingPps = coalition.IsBuildingPps
        });
    }

    [HttpPost("treasury/deposit")]
    public async Task<ActionResult<ServiceResult>> DepositTreasury([FromBody] TreasuryTransferDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (dto.Gold < 0 || dto.Budulec < 0) return BadRequest("Nieprawidłowa kwota.");
        if (dto.Gold == 0 && dto.Budulec == 0) return BadRequest("Podaj kwotę do wpłaty.");

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        long gold = Math.Min(dto.Gold, kingdom.Gold);
        long budulec = Math.Min(dto.Budulec, kingdom.BudulecStored);
        kingdom.Gold -= gold;
        kingdom.BudulecStored -= budulec;
        coalition.TreasuryGold += gold;
        coalition.TreasuryBudulec += budulec;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Wpłacono do kasy: {gold:N0} złota, {budulec:N0} budulca." });
    }

    [HttpPost("treasury/withdraw")]
    public async Task<ActionResult<ServiceResult>> WithdrawTreasury([FromBody] TreasuryTransferDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może wypłacać z kasy.");
        if (dto.Gold < 0 || dto.Budulec < 0) return BadRequest("Nieprawidłowa kwota.");

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        long gold = Math.Min(dto.Gold, coalition.TreasuryGold);
        long budulec = Math.Min(dto.Budulec, coalition.TreasuryBudulec);
        coalition.TreasuryGold -= gold;
        coalition.TreasuryBudulec -= budulec;
        kingdom.Gold += gold;
        kingdom.BudulecStored += budulec;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Wypłacono z kasy: {gold:N0} złota, {budulec:N0} budulca." });
    }

    [HttpPost("treasury/fund-pps")]
    public async Task<ActionResult<ServiceResult>> FundPpsFromTreasury([FromBody] FundPpsDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może finansować PPS z kasy.");
        if (dto.Budulec <= 0) return BadRequest("Podaj ilość budulca.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");
        if (!coalition.IsBuildingPps) return BadRequest("Koalicja nie rozpoczęła budowy PPS.");

        long give = Math.Min(dto.Budulec, coalition.TreasuryBudulec);
        if (give <= 0) return BadRequest("Kasa nie ma budulca.");

        coalition.TreasuryBudulec -= give;
        coalition.PpsBudulec += give;
        coalition.PSOProgress = Math.Min(100m, (decimal)coalition.PpsBudulec / PpsCost * 100m);

        long land = coalition.Members.Sum(m => (long)m.Land);
        if (coalition.PpsBudulec >= PpsCost && land >= PpsRequiredLand)
        {
            await _context.SaveChangesAsync();
            await EraConcluder.ConcludeAsync(_context, coalition);
            return Ok(new ServiceResult { Success = true, Message = "Pałac Sądu Ostatecznego ukończony z kasy koalicji! Era zakończona." });
        }

        await _context.SaveChangesAsync();
        return Ok(new ServiceResult { Success = true, Message = $"Przekazano z kasy {give:N0} budulca na PPS. Postęp: {coalition.PSOProgress:0.##}%." });
    }

    // === Wybory Imperatora (docs/MECHANIKA.md §12) ===

    [HttpGet("election")]
    public async Task<ActionResult<ElectionDto>> GetElection()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return Ok(new ElectionDto { HasCoalition = false });

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return NotFound("Koalicja nie istnieje.");

        var members = await _context.Kingdoms
            .Where(k => k.CoalitionId == kingdom.CoalitionId).ToListAsync();
        var memberIds = members.Select(m => m.Id).ToHashSet();
        var voteCounts = members
            .Where(m => m.ImperatorVoteForKingdomId != null && memberIds.Contains(m.ImperatorVoteForKingdomId.Value))
            .GroupBy(m => m.ImperatorVoteForKingdomId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new ElectionDto
        {
            HasCoalition = true,
            CurrentImperatorId = coalition.LeaderKingdomId,
            CurrentImperatorName = members.FirstOrDefault(m => m.Id == coalition.LeaderKingdomId)?.Name,
            MyVoteKingdomId = kingdom.ImperatorVoteForKingdomId,
            TotalMembers = members.Count,
            Candidates = members.Select(m => new ElectionCandidateDto
            {
                KingdomId = m.Id,
                Name = m.Name,
                Votes = voteCounts.GetValueOrDefault(m.Id, 0),
                IsImperator = coalition.LeaderKingdomId == m.Id,
                IsMyVote = kingdom.ImperatorVoteForKingdomId == m.Id
            }).OrderByDescending(c => c.Votes).ThenBy(c => c.Name).ToList()
        });
    }

    [HttpPost("vote")]
    public async Task<ActionResult<ServiceResult>> Vote([FromBody] VoteImperatorDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");

        var candidate = await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.Id == dto.CandidateKingdomId && k.CoalitionId == kingdom.CoalitionId);
        if (candidate == null) return BadRequest("Kandydat nie należy do Twojej koalicji.");

        kingdom.ImperatorVoteForKingdomId = candidate.Id;

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition != null) await RecomputeImperatorAsync(coalition);

        await _context.SaveChangesAsync();
        return Ok(new ServiceResult { Success = true, Message = $"Oddano głos na {candidate.Name}." });
    }

    /// <summary>Przelicza wybory: kandydat z największą liczbą głosów zostaje Imperatorem (remis = bez zmian).</summary>
    private async Task RecomputeImperatorAsync(Coalition coalition)
    {
        var members = await _context.Kingdoms.Where(k => k.CoalitionId == coalition.Id).ToListAsync();
        if (members.Count == 0) return;

        var memberIds = members.Select(m => m.Id).ToHashSet();
        var tally = members
            .Where(m => m.ImperatorVoteForKingdomId != null && memberIds.Contains(m.ImperatorVoteForKingdomId.Value))
            .GroupBy(m => m.ImperatorVoteForKingdomId!.Value)
            .Select(g => new { KingdomId = g.Key, Votes = g.Count() })
            .OrderByDescending(x => x.Votes)
            .ToList();
        if (tally.Count == 0) return;

        int topVotes = tally[0].Votes;
        var leaders = tally.Where(x => x.Votes == topVotes).ToList();
        if (leaders.Count != 1) return;                       // remis — władza bez zmian

        int newImperatorId = leaders[0].KingdomId;
        if (coalition.LeaderKingdomId == newImperatorId) return;

        var oldImperator = members.FirstOrDefault(m => m.CoalitionRole == "Imperator");
        if (oldImperator != null) oldImperator.CoalitionRole = "Member";

        var newImperator = members.First(m => m.Id == newImperatorId);
        newImperator.CoalitionRole = "Imperator";
        coalition.LeaderKingdomId = newImperatorId;
    }

    // === Wojny koalicji (docs/MECHANIKA.md §12, §14.4) ===

    [HttpGet("wars")]
    public async Task<ActionResult<List<WarDto>>> GetWars()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");

        var wars = await _context.Wars
            .Where(w => w.EraId == kingdom.EraId && w.Status == "Active")
            .Include(w => w.DeclaringCoalition)
            .Include(w => w.TargetCoalition)
            .OrderByDescending(w => w.DeclaredAt)
            .ToListAsync();

        int? myCoalition = kingdom.CoalitionId;
        return Ok(wars.Select(w =>
        {
            bool mine = w.DeclaringCoalitionId == myCoalition;
            bool involved = mine || w.TargetCoalitionId == myCoalition;
            return new WarDto
            {
                Id = w.Id,
                DeclaringCoalitionId = w.DeclaringCoalitionId,
                DeclaringName = w.DeclaringCoalition.Name,
                TargetCoalitionId = w.TargetCoalitionId,
                TargetName = w.TargetCoalition.Name,
                DeclaredAt = w.DeclaredAt,
                IsMyDeclaration = mine,
                OpponentCoalitionId = involved ? (mine ? w.TargetCoalitionId : w.DeclaringCoalitionId) : 0,
                OpponentName = involved ? (mine ? w.TargetCoalition.Name : w.DeclaringCoalition.Name) : string.Empty
            };
        }).ToList());
    }

    [HttpPost("war/declare")]
    public async Task<ActionResult<ServiceResult>> DeclareWar([FromBody] DeclareWarDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może wypowiadać wojny.");

        // Wypowiedzenie wojny możliwe tylko do 20:00 (czas serwera)
        if (DateTime.UtcNow.Hour >= 20)
            return BadRequest("Wojnę można wypowiedzieć tylko do godziny 20:00.");

        if (dto.TargetCoalitionId == kingdom.CoalitionId)
            return BadRequest("Nie możesz wypowiedzieć wojny własnej koalicji.");

        var targetCoalition = await _context.Coalitions
            .FirstOrDefaultAsync(c => c.Id == dto.TargetCoalitionId && c.EraId == kingdom.EraId);
        if (targetCoalition == null) return NotFound("Nie znaleziono koalicji celu.");

        if (await WarHelper.AreAtWarAsync(_context, kingdom.CoalitionId, dto.TargetCoalitionId))
            return BadRequest("Twoja koalicja jest już w stanie wojny z tą koalicją.");

        _context.Wars.Add(new War
        {
            EraId = kingdom.EraId,
            DeclaringCoalitionId = kingdom.CoalitionId.Value,
            TargetCoalitionId = dto.TargetCoalitionId,
            Status = "Active"
        });
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = $"Wypowiedziano wojnę koalicji '{targetCoalition.Name}'!" });
    }

    [HttpPost("war/{id}/end")]
    public async Task<ActionResult<ServiceResult>> EndWar(int id)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom == null) return NotFound("Nie znaleziono księstwa.");
        if (kingdom.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (kingdom.CoalitionRole != "Imperator") return BadRequest("Tylko Imperator może zakończyć wojnę.");

        var war = await _context.Wars.FirstOrDefaultAsync(w => w.Id == id && w.Status == "Active");
        if (war == null) return NotFound("Nie znaleziono aktywnej wojny.");
        if (war.DeclaringCoalitionId != kingdom.CoalitionId && war.TargetCoalitionId != kingdom.CoalitionId)
            return BadRequest("Twoja koalicja nie bierze udziału w tej wojnie.");

        war.Status = "Ended";
        war.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Zawarto pokój — wojna zakończona." });
    }

    // ====================== TABLICA OGŁOSZEŃ (docs/TODO.md A5) ======================

    /// <summary>Czy księstwo może edytować tablicę ogłoszeń (Imperator/Głównodowodzący).</summary>
    private static bool CanEditAnnouncements(Kingdom k) =>
        k.CoalitionRole is "Imperator" or "MainCommander";

    [HttpGet("announcements")]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAnnouncements()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return Ok(new List<AnnouncementDto>());

        var items = await _context.CoalitionAnnouncements
            .Where(a => a.CoalitionId == kingdom.CoalitionId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                ContentHtml = a.ContentHtml,
                AuthorName = a.AuthorName,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("announcements")]
    public async Task<ActionResult> CreateAnnouncement([FromBody] SaveAnnouncementDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (!CanEditAnnouncements(kingdom))
            return BadRequest("Tylko Imperator i Głównodowodzący mogą edytować tablicę ogłoszeń.");
        if (string.IsNullOrWhiteSpace(dto.ContentHtml))
            return BadRequest("Treść ogłoszenia nie może być pusta.");

        _context.CoalitionAnnouncements.Add(new CoalitionAnnouncement
        {
            CoalitionId = kingdom.CoalitionId.Value,
            Title = dto.Title,
            ContentHtml = dto.ContentHtml,
            AuthorName = kingdom.Name
        });
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Ogłoszenie dodane." });
    }

    [HttpPut("announcements/{id:int}")]
    public async Task<ActionResult> UpdateAnnouncement(int id, [FromBody] SaveAnnouncementDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (!CanEditAnnouncements(kingdom))
            return BadRequest("Tylko Imperator i Głównodowodzący mogą edytować tablicę ogłoszeń.");

        var item = await _context.CoalitionAnnouncements
            .FirstOrDefaultAsync(a => a.Id == id && a.CoalitionId == kingdom.CoalitionId);
        if (item == null) return NotFound("Nie znaleziono ogłoszenia.");
        if (string.IsNullOrWhiteSpace(dto.ContentHtml))
            return BadRequest("Treść ogłoszenia nie może być pusta.");

        item.Title = dto.Title;
        item.ContentHtml = dto.ContentHtml;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Ogłoszenie zaktualizowane." });
    }

    [HttpDelete("announcements/{id:int}")]
    public async Task<ActionResult> DeleteAnnouncement(int id)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (!CanEditAnnouncements(kingdom))
            return BadRequest("Tylko Imperator i Głównodowodzący mogą edytować tablicę ogłoszeń.");

        var item = await _context.CoalitionAnnouncements
            .FirstOrDefaultAsync(a => a.Id == id && a.CoalitionId == kingdom.CoalitionId);
        if (item == null) return NotFound("Nie znaleziono ogłoszenia.");

        _context.CoalitionAnnouncements.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Ogłoszenie usunięte." });
    }

    // ====================== PANTEON (docs/MECHANIKA.md §12) ======================

    /// <summary>Sala chwały: zwycięskie koalicje zakończonych er.</summary>
    [HttpGet("pantheon")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPantheon()
    {
        var entries = await _context.Pantheons
            .Include(p => p.Era)
            .Include(p => p.Coalition).ThenInclude(c => c.Leader)
            .OrderByDescending(p => p.VictoryDate)
            .Select(p => new
            {
                p.Id,
                EraName = p.Era.Name,
                CoalitionName = p.Coalition.Name,
                CoalitionTag = p.Coalition.Tag,
                ImperatorName = p.Coalition.Leader != null ? p.Coalition.Leader.Name : null,
                p.VictoryDate
            })
            .ToListAsync();

        return Ok(entries);
    }

    // ====================== WSPÓLNE HASŁO KOALICJI (docs/TODO.md A4) ======================

    /// <summary>
    /// Ustawia lub czyści wspólne hasło koalicji. Pozwala ono zalogować się na dowolne
    /// księstwo koalicji jego loginem + tym hasłem. Tylko Imperator/Głównodowodzący.
    /// </summary>
    [HttpPost("shared-password")]
    public async Task<ActionResult> SetSharedPassword([FromBody] SharedPasswordDto dto)
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return BadRequest("Nie należysz do koalicji.");
        if (!CanEditAnnouncements(kingdom))
            return BadRequest("Tylko Imperator i Głównodowodzący mogą ustawić wspólne hasło.");

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        if (coalition == null) return BadRequest("Nie znaleziono koalicji.");

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            coalition.SharedPasswordHash = null;
            await _context.SaveChangesAsync();
            return Ok(new ServiceResult { Success = true, Message = "Wspólne hasło koalicji wyłączone." });
        }

        if (dto.Password.Length < 6)
            return BadRequest("Wspólne hasło musi mieć co najmniej 6 znaków.");

        coalition.SharedPasswordHash = PasswordHasher.Hash(dto.Password);
        await _context.SaveChangesAsync();
        return Ok(new ServiceResult
        {
            Success = true,
            Message = "Wspólne hasło ustawione. Każde księstwo koalicji można teraz zalogować jego loginem + tym hasłem."
        });
    }

    /// <summary>Czy koalicja ma ustawione wspólne hasło (bez ujawniania hasła).</summary>
    [HttpGet("shared-password")]
    public async Task<ActionResult> GetSharedPasswordStatus()
    {
        var kingdom = await GetCurrentKingdom();
        if (kingdom?.CoalitionId == null) return Ok(new { enabled = false, canManage = false });

        var coalition = await _context.Coalitions.FirstOrDefaultAsync(c => c.Id == kingdom.CoalitionId);
        return Ok(new
        {
            enabled = coalition?.SharedPasswordHash != null,
            canManage = CanEditAnnouncements(kingdom)
        });
    }

    private async Task<Kingdom?> GetCurrentKingdom()
    {
        var userId = GetUserId();
        return await _context.Kingdoms
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Era.IsActive);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
