namespace DBGuard.Contracts.Models.Settings;

public class EmailSendingRuleData
{
    public string SmtpHost { get; set; } = default!;

    public int Port { get; set; }

    public string Username { get; set; } = default!;
    
    public string PasswordEncrypted { get; set; } = default!;

    public bool UseTls { get; set; } = true;

    public string FromEmail { get; set; } = default!;
    
    public string Recipients { get; set; } = default!;
}