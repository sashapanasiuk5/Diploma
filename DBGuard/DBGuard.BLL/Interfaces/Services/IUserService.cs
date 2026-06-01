using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.BLL.Interfaces.Services;

public interface IUserService
{
    public Task<User?> ValidateUser(string username, string password);
    
    public Task<User> SetRoot(int rootUserId, string password);
}