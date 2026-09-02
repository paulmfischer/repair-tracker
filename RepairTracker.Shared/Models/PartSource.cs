using System.ComponentModel.DataAnnotations;

namespace RepairTracker.Models;

public enum PartSource
{
    [Display(Name = "Bought")]
    Bought,
    [Display(Name = "Donor")]
    Donor,
    [Display(Name = "Had on Hand")]
    HadOnHand,
    [Display(Name = "Other")]
    Other
}
