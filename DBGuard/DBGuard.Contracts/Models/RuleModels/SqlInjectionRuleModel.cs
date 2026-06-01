using System.Text.Json;
using DBGuard.Contracts.Models.GalliumData;
using DBGuard.Contracts.Models.GalliumData.Filters;
using DBGuard.Contracts.Models.GalliumData.Filters.Parameters;
using DBGuard.Contracts.Models.RuleModels.RulesData;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.Contracts.Models.RuleModels;

public class SqlInjectionRuleModel : Rule
{
    public SqlInjectionRuleData RuleData { get; set; }

    public SqlInjectionRuleModel()
    {
        Key = (byte)AlertType.SQLInjection;
        RuleData = new SqlInjectionRuleData();
    }
    
    public SqlInjectionRuleModel(Rule primeEntity)
    {
        Key = primeEntity.Key;
        IsEnabled = primeEntity.IsEnabled;
        Data = primeEntity.Data;

        RuleData = JsonSerializer.Deserialize<SqlInjectionRuleData>(primeEntity.Data) ?? throw new InvalidOperationException();
    }

    public Rule ToEntity()
    {
        Data = JsonSerializer.Serialize(RuleData);
        return this;
    }
    
    public SqlInjectionFilterDto ToFilterDto()
    {
        return new SqlInjectionFilterDto
        {
            Active = IsEnabled,
            FilterType = "SQLInjectionFilter",
            FilterGroup = FilterGroup.Request,
            Parameters = new SqlInjectionParametersDto
            {
                Action = (int)RuleData.SqlInjectionAction,
                Threshold = RuleData.SqlInjectionConfidenceThreshold.ToString()
            }
        };
    }
}