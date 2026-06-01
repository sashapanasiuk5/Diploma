namespace DBGuard.BLL.Interfaces.Services;

public interface IAlertService
{
    public Task SendSQLInjectionAlertAsync(string query, float accuracy, string username, string ipAddress);

    public Task SendBulkOperationAlertAsync(string tableNames, long rowCount, string username, string ipAddress);
}