using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class CachingSettingsService(ApiSettingsService api, IndexedDbStore cache, OutboxStore outbox) : ISettingsService
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
        catch (HttpRequestException ex) when (ex.StatusCode is null)
        {
            var cached = await cache.GetAllAsync<AppSettings>(StoreName);
            return cached.FirstOrDefault() ?? new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        try
        {
            await api.SaveAsync(settings);
            await cache.PutAsync(StoreName, settings);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null)
        {
            await cache.PutAsync(StoreName, settings);
            await outbox.EnqueueSaveSettingsAsync(settings);
        }
    }
}
