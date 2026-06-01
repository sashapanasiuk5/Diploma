using System.Text.Json;
using DBGuard.Contracts.Models.GalliumData;
using DBGuard.Contracts.Models.GalliumData.Filters;
using DBGuard.Contracts.Models.GalliumData.Filters.Parameters;
using DBGuard.Contracts.Models.RuleModels.RulesData;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.Contracts.Models.RuleModels;

public class BulkOperationsRuleModel: Rule
{
    public List<BulkOperationsRuleData> RuleData { get; set; } = new();
    
    public BulkOperationsRuleModel()
    {
        Key = (byte)AlertType.BulkOperations;
    }

    public BulkOperationsRuleModel(Rule primeEntity)
    {
        Key = primeEntity.Key;
        IsEnabled = primeEntity.IsEnabled;
        Data = primeEntity.Data;

        RuleData = JsonSerializer.Deserialize<List<BulkOperationsRuleData>>(Data) ?? throw new InvalidOperationException();
    }
    
    public Rule ToEntity()
    {
        Data = JsonSerializer.Serialize(RuleData);
        return this;
    }
    
    public BulkOperationsFilterDto ToFilterDto()
    {
        return new BulkOperationsFilterDto
        {
            Active = IsEnabled,
            FilterType = "BulkOperationsFilter",
            FilterGroup = FilterGroup.Duplex,
            Parameters = new BulkOperationsParametersDto()
            {
                Limits = JsonSerializer.Serialize(RuleData)
            }
        };
    }
}