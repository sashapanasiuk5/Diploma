namespace DBGuard.Contracts.Models.RuleModels.RulesData;

public class BruteForceRuleData
{
    public string AuditLogFilePath { get; set; }
    public int MaxAttemptsPerUser { get; set; }
    
    public int MaxAttemptsPerIP { get; set; }
    
    public int TimeWindowMinutes { get; set; }
}