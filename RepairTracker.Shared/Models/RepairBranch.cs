namespace RepairTracker.Models;

// Describes a status that branches off the main repair flow, plus the UI actions to enter and
// leave it. Adding a future branch is one more entry here rather than new page logic.
public record RepairBranch(
    RepairStatus BranchStatus,
    string EnterLabel,
    string AlertText,
    string ReturnLabel,
    RepairStatus ReturnStatus,
    Func<RepairStatus, bool> AvailableFrom)
{
    public static readonly RepairBranch[] All =
    [
        new(RepairStatus.Parts,
            EnterLabel: "Mark as Parts",
            AlertText: "This item has been converted to Parts inventory and is no longer moving through the repair flow.",
            ReturnLabel: "Return to Repair Flow",
            ReturnStatus: RepairStatus.Diagnosis,
            AvailableFrom: s => s is not (RepairStatus.Intake or RepairStatus.Sold))
    ];
}
