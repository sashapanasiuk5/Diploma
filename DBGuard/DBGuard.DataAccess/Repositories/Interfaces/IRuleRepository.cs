using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface IRuleRepository
{
    Task<List<Rule>> GetAllRules();
    
    Task SaveRules(List<Rule> rules);
}