namespace DBGuard.DataAccess.DTOs;

public record SqlAuditEvent(
    DateTime EventTime,
    string ActionId,
    bool Succeeded,
    string ServerPrincipalName,
    string ClientIp,
    string ApplicationName,
    string Statement
);