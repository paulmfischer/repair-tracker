using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class CachingItemService(ApiItemService api, IndexedDbStore cache, OutboxStore outbox, ConnectivityService connectivity) : IItemService
{
    private const string StoreName = "items";

    public async Task<List<Item>> GetAllAsync()
    {
        try
        {
            var items = await api.GetAllAsync();
            connectivity.ReportOnline();
            foreach (var item in items)
            {
                await cache.PutAsync(StoreName, item);
            }
            return items;
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            return await cache.GetAllAsync<Item>(StoreName);
        }
    }

    public async Task<Item?> GetByIdAsync(string id)
    {
        try
        {
            var item = await api.GetByIdAsync(id);
            connectivity.ReportOnline();
            if (item is not null)
            {
                await cache.PutAsync(StoreName, item);
            }
            return item;
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            return await cache.GetAsync<Item>(StoreName, id);
        }
    }

    public async Task CreateAsync(Item item)
    {
        try
        {
            await api.CreateAsync(item);
            connectivity.ReportOnline();
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            await cache.PutAsync(StoreName, item);
            await outbox.EnqueueCreateAsync(item);
        }
    }

    public async Task UpdateAsync(Item item)
    {
        try
        {
            await api.UpdateAsync(item);
            connectivity.ReportOnline();
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            await cache.PutAsync(StoreName, item);
            await outbox.EnqueueUpdateAsync(item);
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await api.DeleteAsync(id);
            connectivity.ReportOnline();
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            await cache.RemoveAsync(StoreName, id);
            await outbox.EnqueueDeleteAsync(id);
        }
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent)
    {
        try
        {
            var stats = await api.GetDashboardStatsAsync(feePercent);
            connectivity.ReportOnline();
            return stats;
        }
        catch (HttpRequestException)
        {
            connectivity.ReportOffline();
            var items = await cache.GetAllAsync<Item>(StoreName);
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
    }
}
