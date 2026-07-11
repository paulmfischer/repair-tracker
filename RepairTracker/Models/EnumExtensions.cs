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

    // Color.Tertiary is repurposed (see MudProviders.razor's theme) as the dedicated Intake
    // color, since it isn't used anywhere else in the app.
    public static Color ToColor(this RepairStatus status) => status switch
    {
        RepairStatus.Intake => Color.Tertiary,
        RepairStatus.Diagnosis => Color.Warning,
        RepairStatus.PartsOrdered => Color.Info,
        RepairStatus.Repaired => Color.Success,
        RepairStatus.Listed => Color.Primary,
        RepairStatus.Sold => Color.Dark,
        _ => Color.Dark
    };
}
