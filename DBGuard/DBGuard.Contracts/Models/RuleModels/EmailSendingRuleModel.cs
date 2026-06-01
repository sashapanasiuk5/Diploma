using System.Text.Json;
using DBGuard.Contracts.Models.Settings;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.Contracts.Models.RuleModels;

public class EmailSendingRuleModel: Rule
{
    public EmailSendingRuleData RuleData { get; set; }

    public EmailSendingRuleModel()
    {
        Key = (byte)RuleType.MailSending;
        RuleData = new EmailSendingRuleData();
    }
    
    public EmailSendingRuleModel(Rule primeEntity)
    {
        Key = primeEntity.Key;
        IsEnabled = primeEntity.IsEnabled;
        Data = primeEntity.Data;

        RuleData = JsonSerializer.Deserialize<EmailSendingRuleData>(primeEntity.Data) ?? throw new InvalidOperationException();
    }

    public Rule ToEntity()
    {
        Data = JsonSerializer.Serialize(RuleData);
        return this;
    }
}