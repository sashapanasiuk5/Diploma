using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.DataAccess.Repositories.Interfaces;

public interface IPreferenceRepository
{
    ValueTask<Preference?> GetPreferenceAsync(int id);

    Task SavePreferenceAsync(Preference preference);
}