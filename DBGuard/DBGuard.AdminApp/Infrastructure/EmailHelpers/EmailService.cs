using System.Net;
using System.Net.Mail;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.BLL.Services;
using DBGuard.Contracts.Models;
using DBGuard.Contracts.Models.Settings;
using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.AdminApp.Infrastructure.EmailHelpers;

public class EmailService: IEmailService
{
    private readonly IEmailRenderer _emailRenderer;
    private readonly IEncryptionService _encryptionService;

    public EmailService(IEmailRenderer emailRenderer, IEncryptionService encryptionService)
    {
        _emailRenderer = emailRenderer;
        _encryptionService = encryptionService;
    }

    public async Task SendAlertEmailAsync(List<Alert> alerts, EmailSendingRuleData smtpSettings)
    {
        if (!alerts.Any()) return;

        var model = new AlertEmailModel
        {
            TotalAlerts = alerts.Count,
            SentAt = DateTime.UtcNow,
            Alerts = alerts
        };

        string htmlBody = await _emailRenderer.RenderEmailAsync("DBGuard.AdminApp.Pages.EmailTemplates.AlertNotification", model);

        using var client = new SmtpClient(smtpSettings.SmtpHost, smtpSettings.Port)
        {
            Credentials = new NetworkCredential(smtpSettings.Username, _encryptionService.Decrypt(smtpSettings.PasswordEncrypted)),
            EnableSsl = smtpSettings.UseTls
        };

        var mail = new MailMessage
        {
            From = new MailAddress(smtpSettings.FromEmail),
            Subject = $"DBGuard Alert: {alerts.Count} New Security Threats",
            Body = htmlBody,
            IsBodyHtml = true
        };
        
        foreach (var recipient in smtpSettings.Recipients.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            mail.To.Add(recipient.Trim());
        }

        await client.SendMailAsync(mail);
    }
}