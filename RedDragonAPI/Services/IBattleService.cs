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

    // Magia
    Task<List<SpellListItemDto>> GetAvailableSpellsAsync(int userId);
    Task<ServiceResult> CastSpellAsync(int userId, CastSpellDto dto);

    // Złodzieje
    Task<List<ThiefActionListItemDto>> GetThiefActionsAsync();
    Task<ServiceResult> SendThievesAsync(int userId, SendThievesDto dto);
}
