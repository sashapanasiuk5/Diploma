using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface ICheckpointRepository
{
    Task<Dictionary<string, DateTime>> GetCheckpointsAsync(DetectionType type, IEnumerable<string> entities);
    Task UpdateCheckpointAsync(DetectionType type, string entityValue, DateTime timestamp);
    
    Task SaveChangesAsync();
}