using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Jobs;
using DBGuard.Contracts.Models.RuleModels;
using Quartz;

namespace DBGuard.BLL.Helpers;

public class EmailJobScheduler(ISchedulerFactory schedulerFactory): IEmailJobScheduler
{
    private static readonly JobKey EmailSendingJobKey =
        new($"{nameof(EmailSendingJob)}-job");

    private static readonly TriggerKey EmailSendingJobTriggerKey =
        new($"{nameof(EmailSendingJob)}-trigger");
    
    public async Task Sync(EmailSendingRuleModel rule)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        
        if (!rule.IsEnabled)
        {
            if (await scheduler.CheckExists(EmailSendingJobKey))
            {
                await scheduler.DeleteJob(EmailSendingJobKey);
            }

            return;
        }

        var jobTrigger = TriggerBuilder.Create()
            .ForJob(EmailSendingJobKey)
            .WithIdentity(EmailSendingJobTriggerKey)
            .UsingJobData(new JobDataMap
            {
                { "smtpSettings", rule.RuleData }
            })
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(20)
                .RepeatForever())
            .Build();
        
        if (!await scheduler.CheckExists(EmailSendingJobKey))
        {
            var job = JobBuilder.Create<EmailSendingJob>()
                .WithIdentity(EmailSendingJobKey)
                .Build();
            
            await scheduler.ScheduleJob(job, jobTrigger);

            return;
        }
        
        await scheduler.RescheduleJob(EmailSendingJobTriggerKey, jobTrigger);
    }
}