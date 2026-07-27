using RepairTracker.Models;

namespace RepairTracker.Services;

public interface ISettingsService
{
    Task<AppSettings> GetAsync();
    Task SaveAsync(AppSettings settings);
}
