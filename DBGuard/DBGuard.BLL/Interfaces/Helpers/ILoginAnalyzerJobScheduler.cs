using DBGuard.Contracts.Models.RuleModels;

namespace DBGuard.BLL.Interfaces.Helpers;

public interface ILoginAnalyzerJobScheduler
{
    Task Sync(BruteForceRuleModel jobConfigs);
}