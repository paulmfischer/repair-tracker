using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MudBlazor;

namespace RepairTracker.Models;

public static class EnumExtensions
{
    public static string ToDisplayString(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }

    public static Color ToColor(this RepairStatus status) => status switch
    {
        RepairStatus.Intake => Color.Default,
        RepairStatus.Diagnosis => Color.Warning,
        RepairStatus.PartsOrdered => Color.Info,
        RepairStatus.Repaired => Color.Success,
        RepairStatus.Listed => Color.Primary,
        RepairStatus.Sold => Color.Dark,
        _ => Color.Default
    };

    // MudChip's Color.Default renders darker than the light gray MudTimelineItem's Color.Default
    // uses for its dot; apply this style to Intake chips so they match the timeline dot.
    public static string? ToChipStyle(this RepairStatus status) =>
        status == RepairStatus.Intake
            ? "background-color:var(--mud-palette-gray-light);color:rgba(0,0,0,0.87)"
            : null;
}
