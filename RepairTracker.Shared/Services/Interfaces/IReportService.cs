namespace RepairTracker.Services;

public interface IReportService
{
    Task<byte[]?> GenerateItemReportAsync(string itemId);
}
