using DBGuard.DataAccess.DTOs;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace DBGuard.DataAccess.Repositories.Implementations;

public class SqlAuditRepository : ISqlAuditRepository
{
    private readonly string _connectionString;

    public SqlAuditRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<SqlAuditEvent>> GetRecentEventsAsync(int minutes, bool isFailedOnly, string auditFilePath)
    {
        var events = new List<SqlAuditEvent>();
        var lookbackTime = DateTime.UtcNow.AddMinutes(-minutes);

        const string sql = @"
            SELECT 
                event_time, 
                action_id, 
                succeeded, 
                server_principal_name, 
                client_ip, 
                application_name, 
                statement
            FROM sys.fn_get_audit_file(@path, DEFAULT, DEFAULT)
            WHERE event_time >= @lookbackTime
                AND(
                    @failedOnly = 0
                    OR succeeded = 0
                )
            ORDER BY event_time DESC";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@path", auditFilePath);
        cmd.Parameters.AddWithValue("@failedOnly", isFailedOnly);
        cmd.Parameters.AddWithValue("@lookbackTime", lookbackTime);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            events.Add(new SqlAuditEvent(
                EventTime: reader.GetDateTime(0),
                ActionId: reader.GetString(1),
                Succeeded: reader.GetBoolean(2),
                ServerPrincipalName: reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ClientIp: reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                ApplicationName: reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Statement: reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            ));
        }

        return events;
    }
}