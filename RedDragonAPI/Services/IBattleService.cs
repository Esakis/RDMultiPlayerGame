using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IBattleService
{
    Task<ServiceResult> QueueAttackAsync(int userId, AttackDto dto);
    Task<List<BattleReportDto>> GetBattleReportsAsync(int userId);
    Task<BattleResult> ExecuteMilitaryAttackAsync(QueuedAction action);
    Task ExecuteThiefActionAsync(QueuedAction action);
    Task ExecuteSpellAsync(QueuedAction action);
}
