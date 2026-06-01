using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.DTOs;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DBGuard.BLL.BruteForceDetection;

public class PerUserDetectionStrategy: IBruteForceDetectionStrategy
{
    private readonly int _maxCountFailedAttempts;
    private readonly int _detectionWindowMinutes;

    private readonly string _alertMessage = "Detected {0} failed attempts for user {1} within {2} minutes";
    
    private readonly ICheckpointRepository _checkpointRepository;


    public PerUserDetectionStrategy(int maxCountFailedAttempts, int detectionWindowMinutes, ICheckpointRepository checkpointRepository)
    {
        _maxCountFailedAttempts = maxCountFailedAttempts;
        _detectionWindowMinutes = detectionWindowMinutes;
        _checkpointRepository = checkpointRepository;
    }

    public async Task<List<Alert>> DetectBruteForce(IEnumerable<SqlAuditEvent> logs)
    {
        var alerts = new List<Alert>();
        var groupedLogs = logs.GroupBy(x => x.ServerPrincipalName ?? "Unknown").ToList();
        
        var checkpoints = await _checkpointRepository.GetCheckpointsAsync(DetectionType.PerUser, groupedLogs.Select(g => g.Key));

        foreach (var group in groupedLogs)
        {
            var lastTime = checkpoints.GetValueOrDefault(group.Key, DateTime.MinValue);
            var times = group.Where(l => l.EventTime > lastTime).Select(l => l.EventTime).OrderBy(t => t).ToList();

            int left = 0;
            for (int right = 0; right < times.Count; right++)
            {
                while ((times[right] - times[left]).TotalMinutes > _detectionWindowMinutes) left++;

                var countOfAttempts = (right - left + 1);

                if (countOfAttempts > _maxCountFailedAttempts)
                {
                    alerts.Add(new Alert { Username = group.Key, Type = AlertType.BruteForce, Description = string.Format(_alertMessage, countOfAttempts, group.Key, _detectionWindowMinutes) });
                    
                    await _checkpointRepository.UpdateCheckpointAsync(DetectionType.PerUser, group.Key, times[right]);
                    break;
                }
            }
        }

        await _checkpointRepository.SaveChangesAsync();
        return alerts;
    }
}