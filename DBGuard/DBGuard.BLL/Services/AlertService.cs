using DBGuard.BLL.Interfaces.Services;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;

namespace DBGuard.BLL.Services;

public class AlertService(IAlertRepository alertRepository): IAlertService
{
    public async Task SendSQLInjectionAlertAsync(string query, float accuracy, string username, string ipAddress)
    {
        var description = $" SQL Injection detected with accuracy of {accuracy} for following query: {query}";
        var alert = new Alert()
        {
            Description = description,
            Type = AlertType.SQLInjection,
            Username = username,
            IpAddress = ipAddress
        };
        
        await alertRepository.InsertAlertAsync(alert);
    }

    public async Task SendBulkOperationAlertAsync(string tableNames, long rowCount, string username, string ipAddress)
    {
        string description = $"Bulk operations detected. {rowCount} row(s) affected. Table names: {tableNames}";
        
        var alert = new Alert()
        {
            Description = description,
            Type = AlertType.BulkOperations,
            Username = username,
            IpAddress = ipAddress
        };
        
        await alertRepository.InsertAlertAsync(alert);
    }
}