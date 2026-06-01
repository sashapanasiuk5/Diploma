using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.DTOs;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DBGuard.BLL.BruteForceDetection;

public class PerIpDetectionStrategy : IBruteForceDetectionStrategy
{
    private readonly int _maxCountFailedAttempts;
    private readonly int _detectionWindowMinutes;

    private const string _alertMessage = "Detected {0} failed attempts from IP {1} within {2} minutes";
    
    private readonly ICheckpointRepository _checkpointRepository;

    public PerIpDetectionStrategy(int maxCountFailedAttempts, int detectionWindowMinutes, ICheckpointRepository checkpointRepository)
    {
        _maxCountFailedAttempts = maxCountFailedAttempts;
        _detectionWindowMinutes = detectionWindowMinutes;
        _checkpointRepository = checkpointRepository;
    }

    public async Task<List<Alert>> DetectBruteForce(IEnumerable<SqlAuditEvent> logs)
    {
        var alerts = new List<Alert>();
        var groupedLogs = logs.GroupBy(x => x.ClientIp ?? "Unknown").ToList();
        
        var checkpoints = await _checkpointRepository.GetCheckpointsAsync(DetectionType.PerIp, groupedLogs.Select(g => g.Key));

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
                    alerts.Add(new Alert { IpAddress = group.Key, Type = AlertType.BruteForce, Description = string.Format(_alertMessage, countOfAttempts, group.Key, _detectionWindowMinutes) });
                    
                    await _checkpointRepository.UpdateCheckpointAsync(DetectionType.PerIp, group.Key, times[right]);
                    break;
                }
            }
        }

        await _checkpointRepository.SaveChangesAsync();
        return alerts;
    }
}