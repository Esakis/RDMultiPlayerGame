using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IKingdomService
{
    Task<KingdomDto?> GetKingdomByUserIdAsync(int userId);
    Task<KingdomDto?> GetKingdomByIdAsync(int kingdomId);
    Task<Kingdom> CreateKingdomAsync(int userId, string name, string race, int eraId);
    Task<ServiceResult> AssignWorkersAsync(int userId, AssignWorkersDto dto);
    Task<ServiceResult> BuyLandAsync(int userId, int amount);
    Task<List<KingdomSummaryDto>> GetAllKingdomsAsync(int eraId);
    Task<ServiceResult> FreezeAsync(int userId);
    Task<ServiceResult> UnfreezeAsync(int userId);
    Task<ServiceResult> SetMetamagicAsync(int userId, string mode);
    Task<ServiceResult> ChargeTotemAsync(int userId, string totem);
    Task<ServiceResult> SetAppliedScienceAsync(int userId, string school);
    Task<ServiceResult> ChangeRaceAsync(int userId, string race);
}
