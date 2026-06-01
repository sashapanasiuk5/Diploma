using DBGuard.Contracts.Models.Settings;
using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.BLL.Interfaces.Services;

public interface IEmailService
{
    Task SendAlertEmailAsync(List<Alert> alerts, EmailSendingRuleData smtpSettings);
}