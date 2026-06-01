using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.BLL.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}