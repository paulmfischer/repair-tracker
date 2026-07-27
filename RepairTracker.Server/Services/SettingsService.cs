using MongoDB.Driver;
using RepairTracker.Data;
using RepairTracker.Models;

namespace RepairTracker.Services;

public class SettingsService : ISettingsService
{
    private readonly MongoDbContext _db;

    public SettingsService(MongoDbContext db) => _db = db;

    public async Task<AppSettings> GetAsync()
    {
        var settings = await _db.Settings.Find(_ => true).FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new AppSettings();
            await _db.Settings.InsertOneAsync(settings);
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings) =>
        await _db.Settings.ReplaceOneAsync(
            s => s.Id == settings.Id,
            settings,
            new ReplaceOptions { IsUpsert = true });
}
