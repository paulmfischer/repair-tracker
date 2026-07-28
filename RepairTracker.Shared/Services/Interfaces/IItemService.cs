using RepairTracker.Models;

namespace RepairTracker.Services;

public interface IItemService
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(string id);
    Task CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(string id);
    Task<DashboardStats> GetDashboardStatsAsync(decimal feePercent);
}

public record DashboardStats(
    decimal TotalEstimatedProfit,
    decimal TotalActualProfit,
    decimal TotalPostage,
    decimal TotalHoursWorked,
    Dictionary<RepairStatus, int> StatusCounts
);
