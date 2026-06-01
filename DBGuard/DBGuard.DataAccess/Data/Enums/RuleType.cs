namespace DBGuard.DataAccess.Data.Enums;

public enum RuleType
{
    SQLInjection = 0,
    
    BruteForce = 1,
    
    BulkOperations = 2,
    
    MailSending = 3
}