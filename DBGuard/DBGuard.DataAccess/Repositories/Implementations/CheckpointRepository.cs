using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.DataAccess.Repositories.Implementations;

public class CheckpointRepository : ICheckpointRepository
{
    private readonly AppDbContext _context;

    public CheckpointRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, DateTime>> GetCheckpointsAsync(DetectionType type, IEnumerable<string> entities)
    {
        return await _context.DetectionCheckpoints
            .Where(c => c.Type == type && entities.Contains(c.EntityValue))
            .ToDictionaryAsync(c => c.EntityValue, c => c.LastAlertTimestamp);
    }

    public async Task UpdateCheckpointAsync(DetectionType type, string entityValue, DateTime timestamp)
    {
        var checkpoint = _context.DetectionCheckpoints.Local
                             .FirstOrDefault(c => c.Type == type && c.EntityValue == entityValue)
                         ?? await _context.DetectionCheckpoints
                             .FirstOrDefaultAsync(c => c.Type == type && c.EntityValue == entityValue);

        if (checkpoint == null)
        {
            _context.DetectionCheckpoints.Add(new DetectionCheckpoint
            {
                Type = type,
                EntityValue = entityValue,
                LastAlertTimestamp = timestamp
            });
        }
        else
        {
            checkpoint.LastAlertTimestamp = timestamp;
        }
    }
    
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}