using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class CachingSettingsService(ApiSettingsService api, IndexedDbStore cache) : ISettingsService
{
    private const string StoreName = "settings";

    public async Task<AppSettings> GetAsync()
    {
        try
        {
            var settings = await api.GetAsync();
            await cache.PutAsync(StoreName, settings);
            return settings;
        }
        catch (HttpRequestException)
        {
            var cached = await cache.GetAllAsync<AppSettings>(StoreName);
            return cached.FirstOrDefault() ?? new AppSettings();
        }
    }

    public Task SaveAsync(AppSettings settings) => api.SaveAsync(settings);
}
