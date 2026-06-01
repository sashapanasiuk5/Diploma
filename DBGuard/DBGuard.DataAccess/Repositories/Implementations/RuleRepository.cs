using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.DataAccess.Repositories.Implementations;

public class RuleRepository(AppDbContext context): IRuleRepository
{
    public Task<List<Rule>> GetAllRules()
    {
        return context.Rules.ToListAsync();
    }

    public async Task SaveRules(List<Rule> rules)
    {
        context.Rules.UpdateRange(rules);
        await context.SaveChangesAsync();
    }
}