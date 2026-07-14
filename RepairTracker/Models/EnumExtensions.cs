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
    // color, since it isn't used anywhere else in the app. Parts doesn't get a MudBlazor Color
    // at all (every other enum value is already used elsewhere in the app) — it's styled
    // directly via ToColorStyle() instead, wherever a status chip/marker is rendered.
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

    private const string PartsHex = "#9CCC65";

    // Inline style override for statuses that don't have a dedicated MudBlazor Color of their
    // own. Apply alongside ToColor() (e.g. `Color="@status.ToColor()" Style="@status.ToColorStyle()"`)
    // — the inline style wins over the Color-driven CSS class for statuses this covers.
    public static string? ToColorStyle(this RepairStatus status) => status switch
    {
        RepairStatus.Parts => $"background-color:{PartsHex};color:#000000;",
        _ => null
    };

    // Same idea as ToColorStyle(), but for outlined buttons/chips where we want the status
    // color as the border/text rather than a filled background.
    public static string? ToOutlinedColorStyle(this RepairStatus status) => status switch
    {
        RepairStatus.Parts => $"color:{PartsHex};border-color:{PartsHex};",
        _ => null
    };
}
