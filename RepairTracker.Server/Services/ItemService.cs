using MongoDB.Bson;
using MongoDB.Driver;
using RepairTracker.Data;
using RepairTracker.Models;

namespace RepairTracker.Services;

public class ItemService : IItemService
{
    private readonly MongoDbContext _db;
    private readonly ILogger<ItemService> _logger;

    public ItemService(MongoDbContext db, ILogger<ItemService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        var items = await _db.Items.Find(_ => true).SortByDescending(i => i.CreatedAt).ToListAsync();
        foreach (var item in items)
        {
            MigrateLegacyParts(item);
        }
        return items;
    }

    public async Task<Item?> GetByIdAsync(string id)
    {
        var item = await _db.Items.Find(i => i.Id == id).FirstOrDefaultAsync();
        if (item is null)
        {
            _logger.LogWarning("Item {ItemId} not found", id);
            return null;
        }

        // Persist the migration now, while the item happens to be loaded, rather than on every
        // GetAllAsync call — bypasses ReplaceOneAsync via UpdateAsync so opening an item doesn't
        // bump UpdatedAt or re-run ClearRepairOnlyFields as a side effect of just viewing it.
        if (MigrateLegacyParts(item))
        {
            await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item);
        }

        return item;
    }

    // Old documents stored a flat "Parts" decimal, now deserialized into LegacyPartsCost via its
    // BsonElement mapping since Parts itself became a computed rollup of PartsList. Folds that
    // value into a single PartsList entry the first time the item is loaded, so nothing is lost.
    private static bool MigrateLegacyParts(Item item)
    {
        if (item.LegacyPartsCost is not > 0 || item.PartsList.Count > 0)
        {
            item.LegacyPartsCost = null;
            return false;
        }

        item.PartsList.Add(new PartLineItem
        {
            Name = "Existing parts cost",
            Quantity = 1,
            UnitCost = item.LegacyPartsCost.Value,
            Source = PartSource.Other
        });
        item.LegacyPartsCost = null;
        return true;
    }

    public async Task CreateAsync(Item item)
    {
        ClearRepairOnlyFields(item);
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        // Upsert rather than insert so a replayed offline-outbox create (same client-minted Id) is
        // idempotent instead of throwing a duplicate-key error if it somehow gets sent twice.
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item, new ReplaceOptions { IsUpsert = true });
        _logger.LogInformation("Item {ItemId} created", item.Id);
    }

    public async Task UpdateAsync(Item item)
    {
        ClearRepairOnlyFields(item);
        item.UpdatedAt = DateTime.UtcNow;
        await _db.Items.ReplaceOneAsync(i => i.Id == item.Id, item);
        _logger.LogInformation("Item {ItemId} updated", item.Id);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Items.DeleteOneAsync(i => i.Id == id);
        _logger.LogInformation("Item {ItemId} deleted", id);
    }

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
