using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepairTracker.Models;

public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public ItemSource Source { get; set; } = ItemSource.Other;
    public RepairStatus Status { get; set; } = RepairStatus.Intake;

    public string PurchaseListingId { get; set; } = string.Empty;
    public string SaleListingId { get; set; } = string.Empty;

    public string OriginalFault { get; set; } = string.Empty;
    public string RepairNotes { get; set; } = string.Empty;

    public decimal Cost { get; set; }
    public decimal Parts { get; set; }
    public decimal EstimatedSellPrice { get; set; }
    public decimal ActualSellPrice { get; set; }
    public decimal Postage { get; set; }
    public decimal HoursWorked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public decimal ResellerFee(decimal feePercent) =>
        Math.Round(EstimatedSellPrice * (feePercent / 100m), 2);

    public decimal EstimatedProfit(decimal feePercent) =>
        EstimatedSellPrice - Cost - Parts - ResellerFee(feePercent);

    [BsonIgnore]
    public decimal NetProfit => ActualSellPrice - Cost - Parts - Postage;

    [BsonIgnore]
    public decimal? HourlyProfit => HoursWorked > 0 ? Math.Round(NetProfit / HoursWorked, 2) : null;
}
