using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public interface IMilitaryService
{
    Task<List<UnitDefinitionDto>> GetAvailableUnitsAsync(int userId);
    Task<List<MilitaryUnitDto>> GetMyArmyAsync(int userId);
    Task<ServiceResult> RecruitUnitsAsync(int userId, RecruitUnitsDto dto);
    Task<ServiceResult> RecruitBatchAsync(int userId, UnitBatchDto dto);
    Task<ServiceResult> DisbandBatchAsync(int userId, UnitBatchDto dto);
    Task<TrainingInfoDto> GetTrainingInfoAsync(int userId);
    Task<ServiceResult> SetTrainingAsync(int userId, SetTrainingDto dto);
}
