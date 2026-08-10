using System.Net.Http.Json;
using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class ApiItemService(HttpClient http) : IItemService
{
    public async Task<List<Item>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<Item>>("api/items") ?? [];

    public async Task<Item?> GetByIdAsync(string id) =>
        await http.GetFromJsonAsync<Item>($"api/items/{id}");

    public async Task CreateAsync(Item item)
    {
        var response = await http.PostAsJsonAsync("api/items", item);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Item>();
        if (created is not null)
        {
            item.Id = created.Id;
            item.CreatedAt = created.CreatedAt;
            item.UpdatedAt = created.UpdatedAt;
        }
    }

    public async Task UpdateAsync(Item item)
    {
        var response = await http.PutAsJsonAsync($"api/items/{item.Id}", item);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string id)
    {
        var response = await http.DeleteAsync($"api/items/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent, decimal perOrderFee) =>
        await http.GetFromJsonAsync<DashboardStats>($"api/items/dashboard-stats?feePercent={feePercent}&perOrderFee={perOrderFee}")
        ?? new DashboardStats(0, 0, 0, 0, new Dictionary<RepairStatus, int>());
}
