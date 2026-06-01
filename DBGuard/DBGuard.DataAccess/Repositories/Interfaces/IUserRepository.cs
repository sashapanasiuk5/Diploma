using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<List<User>> GetAllUsers();
    public Task<User?> GetUserByIdAsync(int userId);
    public Task<User?> GetUserByUsername(string username);
    
    public Task UpdateUserAsync(User user);
    
    public Task AddUserAsync(User user);

    public Task ToggleUserStatusAsync(int userId);
}