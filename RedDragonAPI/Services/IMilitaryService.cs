using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public interface IMilitaryService
{
    Task<List<UnitDefinitionDto>> GetAvailableUnitsAsync(int userId);
    Task<List<MilitaryUnitDto>> GetMyArmyAsync(int userId);
    Task<ServiceResult> RecruitUnitsAsync(int userId, RecruitUnitsDto dto);
}
