using System.ComponentModel.DataAnnotations;

namespace RepairTracker.Models;

public enum RepairStatus
{
    [Display(Name = "Intake")]
    Intake,
    [Display(Name = "Diagnosis")]
    Diagnosis,
    [Display(Name = "Parts Ordered")]
    PartsOrdered,
    [Display(Name = "Repaired")]
    Repaired,
    [Display(Name = "Listed")]
    Listed,
    [Display(Name = "Sold")]
    Sold
}
