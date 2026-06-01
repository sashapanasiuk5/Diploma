using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.Contracts.Models.RuleModels.RulesData;

public class SqlInjectionRuleData
{
    public SqlInjectionAction SqlInjectionAction { get; set; }
    
    public float SqlInjectionConfidenceThreshold { get; set; }
}