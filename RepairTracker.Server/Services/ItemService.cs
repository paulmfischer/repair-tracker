using MongoDB.Bson;
using MongoDB.Driver;
using RepairTracker.Data;
using RepairTracker.Models;

namespace RepairTracker.Services;

public class ItemService : IItemService
{
    private readonly MongoDbContext _db;

    public ItemService(MongoDbContext db) => _db = db;

    public async Task<List<Item>> GetAllAsync() =>
        await _db.Items.Find(_ => true).SortByDescending(i => i.CreatedAt).ToListAsync();

    public async Task<Item?> GetByIdAsync(string id) =>
        await _db.Items.Find(i => i.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Item item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        // Upsert rather than insert so a replayed offline-outbox create (same client-minted Id) is
        // idempotent instead of throwing a duplicate-key error if it somehow gets sent twice.
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item, new ReplaceOptions { IsUpsert = true });
    }

    public async Task UpdateAsync(Item item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item);
    }

    public async Task DeleteAsync(string id) =>
        await _db.Items.DeleteOneAsync(i => i.Id == id);

    public async Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent)
    {
        var items = await GetAllAsync();

        var statusCounts = Enum.GetValues<RepairStatus>()
            .ToDictionary(s => s, s => items.Count(i => i.Status == s));

        return new DashboardStats(
            TotalEstimatedProfit: items.Sum(i => i.EstimatedProfit(feePercent)),
            TotalActualProfit: items.Sum(i => i.NetProfit),
            TotalPostage: items.Sum(i => i.Postage),
            TotalHoursWorked: items.Sum(i => i.HoursWorked),
            StatusCounts: statusCounts
        );
    }

    // Matches items with at least one note whose ImagePaths/ThumbnailPaths counts differ,
    // so a re-run only touches notes that still need thumbnails generated or backfilled.
    public async Task<List<Item>> GetItemsNeedingThumbnailsAsync()
    {
        var filter = new BsonDocument("$expr", new BsonDocument("$anyElementTrue",
            new BsonDocument("$map", new BsonDocument
            {
                { "input", "$Notes" },
                { "as", "n" },
                { "in", new BsonDocument("$ne", new BsonArray
                    {
                        new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$$n.ImagePaths", new BsonArray() })),
                        new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$$n.ThumbnailPaths", new BsonArray() }))
                    })
                }
            })
        ));

        return await _db.Items.Find(filter).ToListAsync();
    }
}
