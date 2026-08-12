using System.ComponentModel.DataAnnotations;

namespace RepairTracker.Models;

public enum RepairStatus
{
    [Display(Name = "Intake")]
    Intake,
    [Display(Name = "Diagnosis")]
    Diagnosis,
    [Display(Name = "Awaiting Parts")]
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
    Parts,

    // Appended after Parts (not inserted earlier) because Status is stored in MongoDB as a raw
    // ordinal int — inserting a member in the middle would silently reinterpret existing data.
    [Display(Name = "Shipped")]
    Shipped,
    [Display(Name = "Completed")]
    Completed
}
