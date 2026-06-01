using DBGuard.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
    
namespace DBGuard.BLL.Jobs;

[DisallowConcurrentExecution]
public class ClearCheckpointsJob : IJob
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClearCheckpointsJob> _logger;

    public ClearCheckpointsJob(AppDbContext context, ILogger<ClearCheckpointsJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting cleanup of old detection checkpoints...");
        
        var expirationDate = DateTime.UtcNow.AddHours(-4);

        var oldCheckpoints = _context.DetectionCheckpoints
            .Where(c => c.LastAlertTimestamp < expirationDate);

        int deletedCount = 0;
        
        try
        {
            deletedCount = await oldCheckpoints.ExecuteDeleteAsync();
            _logger.LogInformation("Successfully cleared {Count} old checkpoints.", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while clearing old checkpoints.");
        }
    }
}