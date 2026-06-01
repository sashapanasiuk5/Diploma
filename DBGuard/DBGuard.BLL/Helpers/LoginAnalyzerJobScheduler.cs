using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Jobs;
using DBGuard.Contracts.Models.RuleModels;
using Quartz;

namespace DBGuard.BLL.Helpers;

public class LoginAnalyzerJobScheduler(ISchedulerFactory schedulerFactory): ILoginAnalyzerJobScheduler
{
    
    private static readonly JobKey AnalyzerJobKey =
        new($"{nameof(LoginAttemptsAnalyzerJob)}-job");

    private static readonly TriggerKey AnalyzerJobTriggerKey =
        new($"{nameof(LoginAttemptsAnalyzerJob)}-trigger");
    
    private static readonly JobKey CleanUpJobKey =
        new($"{nameof(ClearCheckpointsJob)}-job");

    private static readonly TriggerKey CleanUpJobTriggerKey =
        new($"{nameof(ClearCheckpointsJob)}-trigger");
    
    public async Task Sync(BruteForceRuleModel rule)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        
        if (!rule.IsEnabled)
        {
            if (await scheduler.CheckExists(AnalyzerJobKey))
            {
                await scheduler.DeleteJob(AnalyzerJobKey);
                await scheduler.DeleteJob(CleanUpJobKey);
            }

            return;
        }
        
        var analyzerTrigger = TriggerBuilder.Create()
            .WithIdentity(AnalyzerJobTriggerKey)
            .ForJob(AnalyzerJobKey)
            .UsingJobData(LoginAttemptsAnalyzerJob.AuditLogPath, rule.RuleData.AuditLogFilePath)
            .UsingJobData(LoginAttemptsAnalyzerJob.MaxLoginAttemptsPerIp, rule.RuleData.MaxAttemptsPerIP)
            .UsingJobData(LoginAttemptsAnalyzerJob.MaxLoginAttemptsPerUser, rule.RuleData.MaxAttemptsPerUser)
            .UsingJobData(LoginAttemptsAnalyzerJob.SlidingWindow, rule.RuleData.TimeWindowMinutes)
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(15)
                .RepeatForever())
            .Build();

        var cleanupTrigger = TriggerBuilder.Create()
            .ForJob(CleanUpJobKey)
            .WithIdentity(CleanUpJobTriggerKey)
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(4)
                .RepeatForever())
            .Build();
        
        if (!await scheduler.CheckExists(AnalyzerJobKey))
        {
            var analyzerJob = JobBuilder.Create<LoginAttemptsAnalyzerJob>()
                .WithIdentity(AnalyzerJobKey)
                .Build();
            
            var cleanUpJob = JobBuilder.Create<ClearCheckpointsJob>()
                .WithIdentity(CleanUpJobKey)
                .Build();

            await scheduler.ScheduleJob(analyzerJob, analyzerTrigger);
            await scheduler.ScheduleJob(cleanUpJob, cleanupTrigger);

            return;
        }
        
        await scheduler.RescheduleJob(AnalyzerJobTriggerKey, analyzerTrigger);
    }
}