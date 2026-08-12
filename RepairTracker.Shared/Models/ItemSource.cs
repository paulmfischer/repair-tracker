using System.ComponentModel.DataAnnotations;

namespace RepairTracker.Models;

public enum ItemSource
{
    [Display(Name = "eBay")]
    eBay,
    [Display(Name = "Game Store")]
    GameStore,
    [Display(Name = "Facebook Marketplace")]
    FacebookMarketplace,
    [Display(Name = "Craigslist")]
    Craigslist,
    [Display(Name = "Local")]
    Local,
    [Display(Name = "Other")]
    Other,

    // Appended last, not inserted earlier — Source is stored in MongoDB as a raw ordinal int.
    // A repair-only item was never bought for resale, so its flow skips Listed/Sold/Shipped
    // (see RepairFlow.RepairJob) and goes straight from Repaired to Completed.
    [Display(Name = "Repair")]
    Repair
}
