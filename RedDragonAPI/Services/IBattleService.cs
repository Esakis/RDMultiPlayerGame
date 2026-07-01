using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IBattleService
{
    Task<ServiceResult> QueueAttackAsync(int userId, AttackDto dto);
    Task<List<PlannedAttackDto>> GetPlannedAttacksAsync(int userId);
    Task<List<PlannedAttackDto>> GetCoalitionPlannedAttacksAsync(int userId);
    Task<ServiceResult<AttackOptionsDto>> GetAttackOptionsAsync(int userId, int kingdomId);
    Task<ServiceResult> CancelPlannedAttackAsync(int userId, int actionId);
    Task<List<BattleReportDto>> GetBattleReportsAsync(int userId);
    Task<List<BattleReportDto>> GetCoalitionBattleReportsAsync(int userId);
    Task<BattleResult> ExecuteMilitaryAttackAsync(QueuedAction action);
    Task ExecuteThiefActionAsync(QueuedAction action);
    Task ExecuteSpellAsync(QueuedAction action);

    // Magia
    Task<List<SpellListItemDto>> GetAvailableSpellsAsync(int userId);
    Task<ServiceResult> CastSpellAsync(int userId, CastSpellDto dto);

    // Złodzieje
    Task<List<ThiefActionListItemDto>> GetThiefActionsAsync();
    Task<ServiceResult> SendThievesAsync(int userId, SendThievesDto dto);
}
