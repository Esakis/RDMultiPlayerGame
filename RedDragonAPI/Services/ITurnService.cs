using RedDragonAPI.Models.DTOs;

namespace RedDragonAPI.Services;

public interface ITurnService
{
    Task<TurnResultDto> UseTurnAsync(int userId);
}
