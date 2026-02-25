using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public interface IResourceService
{
    Task GenerateResourcesForKingdomAsync(Kingdom kingdom);
    Task GenerateResourcesForAllAsync();
}
