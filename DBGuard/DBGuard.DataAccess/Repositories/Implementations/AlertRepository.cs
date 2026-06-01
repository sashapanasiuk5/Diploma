using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.DataAccess.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Alert>> GetAlertListAsync(
        DateTime? from,
        DateTime? to,
        AlertType? type,
        string? username,
        string? ipAddress,
        string? search)
    {
        var query = _context.Alerts.AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(a => a.Username == username);

        if (!string.IsNullOrWhiteSpace(ipAddress))
            query = query.Where(a => a.IpAddress == ipAddress);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Description.Contains(search));

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task InsertAlertAsync(Alert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;

        await _context.Alerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }
    
    public async Task InsertAlertsAsync(List<Alert> alerts)
    {
        foreach (var alert in alerts)
        {
            alert.CreatedAt = DateTime.UtcNow;
            await _context.Alerts.AddAsync(alert);
        }

        await _context.SaveChangesAsync();
    }
}