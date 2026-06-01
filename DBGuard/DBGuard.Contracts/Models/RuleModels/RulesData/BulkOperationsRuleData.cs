namespace DBGuard.Contracts.Models.RuleModels.RulesData;

public class BulkOperationsRuleData
{
    public string TableName { get; set; } = string.Empty;
    public int Threshold { get; set; } = 0;
}