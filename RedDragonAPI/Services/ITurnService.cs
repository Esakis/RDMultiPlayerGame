using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public interface ITurnService
{
    Task<ServiceResult> UseTurnAsync(int userId);
}
