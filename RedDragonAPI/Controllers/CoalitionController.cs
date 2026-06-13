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
            .Select(c => new CoalitionDto
            {
                Id = c.Id,
                Name = c.Name,
                Tag = c.Tag,
                LeaderKingdomId = c.LeaderKingdomId,
                LeaderName = c.Leader != null ? c.Leader.Name : null,
                MemberCount = c.Members.Count,
                MaxMembers = c.MaxMembers,
                PSOProgress = c.PSOProgress,
                Members = c.Members.Select(m => new KingdomSummaryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Race = m.Race,
                    Land = m.Land,
                    Population = m.Population,
                    CoalitionRole = m.CoalitionRole
                }).ToList()
            })
            .ToListAsync();

        return Ok(coalitions);
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

        if (kingdom.CoalitionId.HasValue)
            return BadRequest("Już należysz do koalicji.");

        var coalition = await _context.Coalitions
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == dto.CoalitionId);

        if (coalition == null)
            return NotFound("Nie znaleziono koalicji.");

        if (coalition.Members.Count >= coalition.MaxMembers)
            return BadRequest("Koalicja jest pełna.");

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
        await _context.SaveChangesAsync();

        return Ok(new ServiceResult { Success = true, Message = "Opuszczono koalicję." });
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
