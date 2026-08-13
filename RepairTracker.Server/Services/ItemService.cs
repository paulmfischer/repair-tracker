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
        ClearRepairOnlyFields(item);
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        // Upsert rather than insert so a replayed offline-outbox create (same client-minted Id) is
        // idempotent instead of throwing a duplicate-key error if it somehow gets sent twice.
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item, new ReplaceOptions { IsUpsert = true });
    }

    public async Task UpdateAsync(Item item)
    {
        ClearRepairOnlyFields(item);
        item.UpdatedAt = DateTime.UtcNow;
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item);
    }

    public async Task DeleteAsync(string id) =>
        await _db.Items.DeleteOneAsync(i => i.Id == id);

    // Purchase/sale listing info, dates, and all financials are hidden on the client for
    // Repair-sourced items (never bought for resale), so enforce that they're actually cleared
    // here rather than just hidden — otherwise switching Source back and forth would resurface
    // stale data the UI never let the user edit or see while it was hidden.
    private static void ClearRepairOnlyFields(Item item)
    {
        if (item.Source != ItemSource.Repair) return;

        item.PurchaseListingId = string.Empty;
        item.SaleListingId = string.Empty;
        item.PurchaseDate = null;
        item.SoldDate = null;

        item.Cost = 0;
        item.PurchaseTax = 0;
        item.Parts = 0;
        item.PurchaseShipping = 0;
        item.PurchaseShippingPaidByMe = true;
        item.EstimatedSellPrice = 0;
        item.ActualSellPrice = 0;
        item.SellerFeeApplies = true;
        item.SalesTax = 0;
        item.Postage = 0;
        item.PostagePaidByMe = false;
        item.HoursWorked = 0;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent, decimal perOrderFee)
    {
        var items = await GetAllAsync();

        var statusCounts = Enum.GetValues<RepairStatus>()
            .ToDictionary(s => s, s => items.Count(i => i.Status == s));

        return new DashboardStats(
            TotalEstimatedProfit: items.Sum(i => i.EstimatedProfit(feePercent, perOrderFee)),
            TotalActualProfit: items.Sum(i => i.NetProfit(feePercent, perOrderFee)),
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
