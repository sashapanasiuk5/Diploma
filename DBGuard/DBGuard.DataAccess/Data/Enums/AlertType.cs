using DBGuard.Common.Attributes;

namespace DBGuard.DataAccess.Data.Enums;

public enum AlertType: byte
{
    [TextRepresentation("SQL Injection")]
    SQLInjection = 0,
    
    [TextRepresentation("Brute Force")]
    BruteForce = 1,
    
    [TextRepresentation("Bulk operations")]
    BulkOperations = 2,
}