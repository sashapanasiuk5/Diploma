using DBGuard.BLL.Interfaces.Services;
using DBGuard.Contracts.Models.Settings;
using DBGuard.DataAccess.Repositories.Interfaces;
using Quartz;

namespace DBGuard.BLL.Jobs;

public class EmailSendingJob(IEmailService emailService, IAlertRepository alertRepository): IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap;

        var smtpSettings = (EmailSendingRuleData)dataMap["smtpSettings"];

        var timeFrom = DateTime.UtcNow.AddSeconds(-20);

        var alerts = await alertRepository.GetAlertListAsync(timeFrom, null, null, null, null, null);
        
        await emailService.SendAlertEmailAsync(alerts, smtpSettings);
    }
}