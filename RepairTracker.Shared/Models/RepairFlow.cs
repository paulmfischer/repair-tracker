namespace RepairTracker.Models;

// The ordered sequence of main-flow statuses (branch statuses like Parts excluded) an item
// steps through, which depends on where it came from: an item taken in purely for repair was
// never bought for resale, so it skips the sale-related statuses entirely.
public static class RepairFlow
{
    public static readonly RepairStatus[] Sale =
        Enum.GetValues<RepairStatus>().Where(s => !s.IsBranch()).ToArray();

    public static readonly RepairStatus[] RepairJob =
    [
        RepairStatus.Intake,
        RepairStatus.Diagnosis,
        RepairStatus.PartsOrdered,
        RepairStatus.Repaired,
        RepairStatus.Completed
    ];

    public static RepairStatus[] For(ItemSource source) =>
        source == ItemSource.Repair ? RepairJob : Sale;
}
