using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface IAlertRepository
{
    Task<List<Alert>> GetAlertListAsync(
        DateTime? from,
        DateTime? to,
        AlertType? type,
        string? username,
        string? ipAddress,
        string? search);

    Task InsertAlertAsync(Alert alert);

    Task InsertAlertsAsync(List<Alert> alerts);
}