using System.Text.Json;
using DBGuard.Contracts.Models.RuleModels.RulesData;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.Contracts.Models.RuleModels;

public class BruteForceRuleModel: Rule
{
    public BruteForceRuleData RuleData { get; set; }
    
    public BruteForceRuleModel()
    {
        Key = (byte)AlertType.BruteForce;
    }

    public BruteForceRuleModel(Rule primeEntity)
    {
        Key = primeEntity.Key;
        IsEnabled = primeEntity.IsEnabled;
        Data = primeEntity.Data;

        RuleData = JsonSerializer.Deserialize<BruteForceRuleData>(Data) ?? throw new InvalidOperationException();
    }
    
    public Rule ToEntity()
    {
        Data = JsonSerializer.Serialize(RuleData);
        return this;
    }
}