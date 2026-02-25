using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IKingdomService
{
    Task<KingdomDto?> GetKingdomByUserIdAsync(int userId);
    Task<KingdomDto?> GetKingdomByIdAsync(int kingdomId);
    Task<Kingdom> CreateKingdomAsync(int userId, string name, int eraId);
    Task<ServiceResult> AssignWorkersAsync(int userId, AssignWorkersDto dto);
    Task<ServiceResult> BuyLandAsync(int userId, int amount);
    Task<List<KingdomSummaryDto>> GetAllKingdomsAsync(int eraId);
}
