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
    Other
}
