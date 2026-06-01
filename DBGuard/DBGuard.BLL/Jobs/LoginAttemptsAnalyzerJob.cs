using DBGuard.BLL.BruteForceDetection;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DBGuard.BLL.Jobs;

[DisallowConcurrentExecution]
public class LoginAttemptsAnalyzerJob(IServiceProvider provider, ILogger<LoginAttemptsAnalyzerJob> logger) : IJob
{
    public const string SlidingWindow = "SlidingWindow";
    
    public const string AuditLogPath = "AuditLogPath";
    
    public const string MaxLoginAttemptsPerUser = "MaxLoginAttemptsPerUser";
    
    public const string MaxLoginAttemptsPerIp = "MaxLoginAttemptsPerIp";
    
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Analyzer Job started.");
        JobDataMap dataMap = context.MergedJobDataMap;
        string auditLogPath = dataMap.GetString(AuditLogPath)!;

        using var scope = provider.CreateScope();
        var sqlAuditRepository = scope.ServiceProvider.GetRequiredService<ISqlAuditRepository>();
        var alertRepository = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
            
        var logs = (await sqlAuditRepository.GetRecentEventsAsync(4 * 60, true, auditLogPath)).ToList();

        var alerts = new List<Alert>();
        foreach (var strategy in GetStrategies(dataMap, scope.ServiceProvider))
        {
            alerts.AddRange(await strategy.DetectBruteForce(logs));
        }
        
        logger.LogInformation($"Found {alerts.Count} alert(s).");

        await alertRepository.InsertAlertsAsync(alerts);
    }

    private List<IBruteForceDetectionStrategy> GetStrategies(JobDataMap dataMap, IServiceProvider serviceProvider)
    {
        int slidingWindow = dataMap.GetInt(SlidingWindow);
        int maxLoginAttemptsPerUser = dataMap.GetIntValue(MaxLoginAttemptsPerUser);
        int maxLoginAttemptsPerIp = dataMap.GetIntValue(MaxLoginAttemptsPerIp);

        var checkpointRepository = serviceProvider.GetRequiredService<ICheckpointRepository>();
        return
        [
            new PerIpDetectionStrategy(maxLoginAttemptsPerIp, slidingWindow, checkpointRepository),
            new PerUserDetectionStrategy(maxLoginAttemptsPerUser, slidingWindow, checkpointRepository)
        ];
    }
}