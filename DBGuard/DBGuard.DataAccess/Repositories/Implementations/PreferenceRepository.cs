using DBGuard.DataAccess.Data;
using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Repositories.Interfaces;

namespace DBGuard.DataAccess.Repositories.Implementations;

public class PreferenceRepository(AppDbContext context): IPreferenceRepository
{
    public ValueTask<Preference?> GetPreferenceAsync(int id)
    {
        return context.Preferences.FindAsync(id);
    }

    public async Task SavePreferenceAsync(Preference preference)
    {
        context.Preferences.Update(preference);
        await context.SaveChangesAsync();
    }
}