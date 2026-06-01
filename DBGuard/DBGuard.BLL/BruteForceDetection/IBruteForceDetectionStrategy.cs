using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.DTOs;

namespace DBGuard.BLL.BruteForceDetection;

public interface IBruteForceDetectionStrategy
{
    Task<List<Alert>> DetectBruteForce(IEnumerable<SqlAuditEvent> logs);
}