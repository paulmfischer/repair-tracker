using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class CachingItemService(ApiItemService api, IndexedDbStore cache) : IItemService
{
    private const string StoreName = "items";

    public async Task<List<Item>> GetAllAsync()
    {
        try
        {
            var items = await api.GetAllAsync();
            foreach (var item in items)
            {
                await cache.PutAsync(StoreName, item);
            }
            return items;
        }
        catch (HttpRequestException)
        {
            return await cache.GetAllAsync<Item>(StoreName);
        }
    }

    public async Task<Item?> GetByIdAsync(string id)
    {
        try
        {
            var item = await api.GetByIdAsync(id);
            if (item is not null)
            {
                await cache.PutAsync(StoreName, item);
            }
            return item;
        }
        catch (HttpRequestException)
        {
            return await cache.GetAsync<Item>(StoreName, id);
        }
    }

    public Task CreateAsync(Item item) => api.CreateAsync(item);

    public Task UpdateAsync(Item item) => api.UpdateAsync(item);

    public Task DeleteAsync(string id) => api.DeleteAsync(id);

    public async Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent)
    {
        try
        {
            return await api.GetDashboardStatsAsync(feePercent);
        }
        catch (HttpRequestException)
        {
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
