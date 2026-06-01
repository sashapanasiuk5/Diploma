using DBGuard.BLL.Helpers;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.Contracts.Models.RuleModels;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Implementations;
using DBGuard.DataAccess.Repositories.Interfaces;

namespace DBGuard.BLL.Services;

public class RuleService(IRuleRepository ruleRepository, IGalliumRepositoryService galliumRepositoryService, ILoginAnalyzerJobScheduler analyzerJobScheduler, IEmailJobScheduler emailJobScheduler): IRuleService
{
    public async Task<(SqlInjectionRuleModel sqlInjectionRule,
        BruteForceRuleModel bruteForceRule,
        BulkOperationsRuleModel bulkOperationsRule,
        EmailSendingRuleModel emailSendingRuleModel)> GetRules()
    {
        var rules = await ruleRepository.GetAllRules();
        
        var sqlInjectionRuleEntity = rules.Find(x => x.Key == (byte)RuleType.SQLInjection);
        var bruteForceRuleEntity = rules.Find(x => x.Key == (byte)RuleType.BruteForce);
        var bulkOperationsRuleEntity = rules.Find(x => x.Key == (byte)RuleType.BulkOperations);
        var emailSendingRuleEntity = rules.Find(x => x.Key == (byte)RuleType.MailSending);
        
        if(sqlInjectionRuleEntity is null || bruteForceRuleEntity is null || bulkOperationsRuleEntity is null || emailSendingRuleEntity is null)
            throw new NullReferenceException();
        
        return (new SqlInjectionRuleModel(sqlInjectionRuleEntity),
            new BruteForceRuleModel(bruteForceRuleEntity), 
            new BulkOperationsRuleModel(bulkOperationsRuleEntity),
            new EmailSendingRuleModel(emailSendingRuleEntity)
            );
    }

    public async Task SaveRules(SqlInjectionRuleModel sqlInjectionRule, BruteForceRuleModel bruteForceRule, BulkOperationsRuleModel bulkOperationsRule, EmailSendingRuleModel emailSendingRuleModel)
    {
        List<Rule> rules = [ sqlInjectionRule.ToEntity(), bruteForceRule.ToEntity(), bulkOperationsRule.ToEntity(), emailSendingRuleModel.ToEntity()];
        await ruleRepository.SaveRules(rules);
        
        await analyzerJobScheduler.Sync(bruteForceRule);
        await emailJobScheduler.Sync(emailSendingRuleModel);
        
        await galliumRepositoryService.UpdateFilters([sqlInjectionRule.ToFilterDto(), bulkOperationsRule.ToFilterDto()]);
    }
}