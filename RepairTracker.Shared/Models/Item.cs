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
    public string? BoardVersion { get; set; }
    public ItemSource Source { get; set; } = ItemSource.eBay;
    public RepairStatus Status { get; set; } = RepairStatus.Intake;

    public string PurchaseListingId { get; set; } = string.Empty;
    public string SaleListingId { get; set; } = string.Empty;

    public string GeneralNotes { get; set; } = string.Empty;
    public string OriginalFault { get; set; } = string.Empty;
    public List<RepairNote> Notes { get; set; } = new();

    public decimal Cost { get; set; }
    public decimal PurchaseTax { get; set; }
    public decimal Parts { get; set; }
    public decimal PurchaseShipping { get; set; }
    public bool PurchaseShippingPaidByMe { get; set; } = true;
    public decimal EstimatedSellPrice { get; set; }
    public decimal ActualSellPrice { get; set; }
    public bool SellerFeeApplies { get; set; } = true;
    public decimal SalesTax { get; set; }
    public decimal Postage { get; set; }
    public bool PostagePaidByMe { get; set; }
    public decimal HoursWorked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? SoldDate { get; set; }

    // eBay charges a flat per-order fee on top of the percentage fee, so both are folded into one line item.
    public decimal ResellerFee(decimal feePercent, decimal perOrderFee) =>
        Math.Round(EstimatedSellPrice * (feePercent / 100m), 2) + perOrderFee;

    public decimal EstimatedProfit(decimal feePercent, decimal perOrderFee) =>
        EstimatedSellPrice - Cost - PurchaseTax - Parts - (PurchaseShippingPaidByMe ? PurchaseShipping : 0) - ResellerFee(feePercent, perOrderFee);

    // If the buyer paid for postage, that amount was part of what eBay charges its fee on.
    // Sales tax collected from the buyer is also part of the order total eBay charges fees on,
    // even though it's never the seller's money.
    public decimal OrderTotal => ActualSellPrice + (PostagePaidByMe ? 0 : Postage) + SalesTax;

    public decimal ActualResellerFee(decimal feePercent, decimal perOrderFee) =>
        SellerFeeApplies ? Math.Round(OrderTotal * (feePercent / 100m), 2) + perOrderFee : 0m;

    // Postage is always an out-of-pocket shipping cost; it only inflates the fee base
    // when the buyer covered it (via OrderTotal), so it nets out of proceeds either way.
    // Sales tax is always passed through to the state, so it's always subtracted back out.
    public decimal SaleProceeds(decimal feePercent, decimal perOrderFee) =>
        OrderTotal - ActualResellerFee(feePercent, perOrderFee) - Postage - SalesTax;

    public decimal NetProfit(decimal feePercent, decimal perOrderFee) =>
        SaleProceeds(feePercent, perOrderFee)
        - Cost
        - PurchaseTax
        - Parts
        - (PurchaseShippingPaidByMe ? PurchaseShipping : 0);

    public decimal? HourlyProfit(decimal feePercent, decimal perOrderFee) =>
        HoursWorked > 0 ? Math.Round(NetProfit(feePercent, perOrderFee) / HoursWorked, 2) : null;
}
