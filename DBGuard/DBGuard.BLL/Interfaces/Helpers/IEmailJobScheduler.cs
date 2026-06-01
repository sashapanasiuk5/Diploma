using DBGuard.Contracts.Models.RuleModels;

namespace DBGuard.BLL.Interfaces.Helpers;

public interface IEmailJobScheduler
{
    Task Sync(EmailSendingRuleModel jobConfigs);
}