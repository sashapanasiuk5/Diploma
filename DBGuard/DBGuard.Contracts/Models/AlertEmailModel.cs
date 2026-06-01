using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.Contracts.Models;

public class AlertEmailModel
{
    public int TotalAlerts { get; set; }
    public DateTime SentAt { get; set; }
    public List<Alert> Alerts { get; set; } = new();
}