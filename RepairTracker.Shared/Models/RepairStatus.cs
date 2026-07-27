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
    Sold,

    // Not part of the normal repair flow — an item branches here (from Diagnosis onward)
    // when it's being kept as spare parts inventory instead of continuing to be repaired.
    [Display(Name = "Parts")]
    Parts
}
