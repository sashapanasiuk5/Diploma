using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBGuard.DataAccess.Repositories.Implementations;

public class UserRepository(AppDbContext dbContext): IUserRepository
{
    public Task<List<User>> GetAllUsers()
    {
        return dbContext.Users.Where(x => x.Role == UserRole.Root || x.Role == UserRole.User).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        return user;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddUserAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public Task ToggleUserStatusAsync(int userId)
    {
        return dbContext.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(x => x.SetProperty(u=>u.IsActive,u => !u.IsActive ));
    }
}