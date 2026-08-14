using MongoDB.Driver;
using RepairTracker.Data;
using RepairTracker.Models;

namespace RepairTracker.Services;

public class SettingsService : ISettingsService
{
    private readonly MongoDbContext _db;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(MongoDbContext db, ILogger<SettingsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AppSettings> GetAsync()
    {
        var settings = await _db.Settings.Find(_ => true).FirstOrDefaultAsync();
        if (settings is null)
        {
            _logger.LogInformation("No settings found, creating defaults");
            settings = new AppSettings();
            await _db.Settings.InsertOneAsync(settings);
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await _db.Settings.ReplaceOneAsync(
            s => s.Id == settings.Id,
            settings,
            new ReplaceOptions { IsUpsert = true });
        _logger.LogInformation("Settings {SettingsId} saved", settings.Id);
    }
}
