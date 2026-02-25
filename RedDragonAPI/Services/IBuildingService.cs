using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public interface IBuildingService
{
    Task<List<BuildingDefinitionDto>> GetAvailableBuildingsAsync(int userId);
    Task<List<BuildingDto>> GetMyBuildingsAsync(int userId);
    Task<ServiceResult> ConstructBuildingAsync(int userId, ConstructBuildingDto dto);
}
