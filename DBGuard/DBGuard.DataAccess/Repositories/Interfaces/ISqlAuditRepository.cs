using DBGuard.DataAccess.DTOs;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface ISqlAuditRepository
{
    /// <summary>
    /// Retrieves audit events that occurred within the specified number of minutes from now.
    /// </summary>
    /// <param name="minutes">The lookback window in minutes.</param>
    /// <param name="auditFilePath">File path to audit logs</param>
    /// <returns>A read-only list of audit events.</returns>
    Task<IEnumerable<SqlAuditEvent>> GetRecentEventsAsync(int minutes,  bool isFailedOnly, string auditFilePath);
}