using DBGuard.Contracts.Models.RuleModels;

namespace DBGuard.BLL.Interfaces.Services;

public interface IRuleService
{
    public Task<(SqlInjectionRuleModel sqlInjectionRule, BruteForceRuleModel bruteForceRule, BulkOperationsRuleModel bulkOperationsRule, EmailSendingRuleModel emailSendingRuleModel)> GetRules();

    public Task SaveRules(SqlInjectionRuleModel sqlInjectionRule, BruteForceRuleModel bruteForceRule,
        BulkOperationsRuleModel bulkOperationsRule, EmailSendingRuleModel emailSendingRuleModel);
}