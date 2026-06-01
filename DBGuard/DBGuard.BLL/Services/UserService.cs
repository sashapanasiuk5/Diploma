using DBGuard.BLL.Interfaces.Services;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using static BCrypt.Net.BCrypt;

namespace DBGuard.BLL.Services;

public class UserService(IUserRepository userRepository): IUserService
{
    public async Task<User?> ValidateUser(string username, string password)
    {
        var user = await userRepository.GetUserByUsername(username);
        
        if (user == null)
            return null;

        if (Verify(password, user.Password))
        {
            return user;
        }
        
        return null;
    }

    public async Task<User> SetRoot(int rootUserId, string password)
    {
        var user = await userRepository.GetUserByIdAsync(rootUserId);
        
        if(user == null)
            throw new ArgumentException();
        
        user.Password = HashPassword(password);
        user.Role = UserRole.Root;
        
        await userRepository.UpdateUserAsync(user);

        return user;
    }
}