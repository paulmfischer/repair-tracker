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

    // Status colors are specified explicitly from MudBlazor's Colors palette (rather than the
    // themed Color enum values like Primary/Success/Warning) so they stay independent of those
    // slots' out-of-the-box use elsewhere in the app (buttons, snackbars, etc). Every status
    // renders via ToColorStyle()/ToOutlinedColorStyle() with Color left at its default.
    public static Color ToColor(this RepairStatus status) => Color.Default;

    private static readonly Dictionary<RepairStatus, (string Hex, string TextHex)> StatusColors = new()
    {
        [RepairStatus.Intake] = (Colors.Purple.Darken1, "#FFFFFF"),
        [RepairStatus.Diagnosis] = (Colors.Orange.Darken1, "#FFFFFF"),
        [RepairStatus.PartsOrdered] = (Colors.DeepOrange.Darken1, "#FFFFFF"),
        [RepairStatus.Repaired] = (Colors.Teal.Darken1, "#FFFFFF"),
        [RepairStatus.Listed] = (Colors.Blue.Darken1, "#FFFFFF"),
        [RepairStatus.Sold] = (Colors.Green.Darken1, "#FFFFFF"),
        [RepairStatus.Parts] = (Colors.Cyan.Darken1, "#FFFFFF"),
        [RepairStatus.Shipped] = (Colors.Indigo.Darken1, "#FFFFFF"),
        [RepairStatus.Completed] = (Colors.Green.Darken3, "#FFFFFF"),
    };

    // Statuses that branch off the main repair flow rather than being a step within it (see
    // RepairBranch.All for the full definition of each branch's enter/return behavior).
    public static readonly RepairStatus[] BranchStatuses = { RepairStatus.Parts };

    public static bool IsBranch(this RepairStatus status) => BranchStatuses.Contains(status);

    // Inline style carrying each status's explicit MudBlazor.Colors swatch. Apply alongside
    // ToColor() (e.g. `Color="@status.ToColor()" Style="@status.ToColorStyle()"`) — the inline
    // style provides the actual color since ToColor() is just Color.Default.
    public static string ToColorStyle(this RepairStatus status)
    {
        var (hex, textHex) = StatusColors[status];
        return $"background-color:{hex};color:{textHex};";
    }

    // Same idea as ToColorStyle(), but for outlined buttons/chips where we want the status
    // color as the border/text rather than a filled background.
    public static string ToOutlinedColorStyle(this RepairStatus status)
    {
        var (hex, _) = StatusColors[status];
        return $"color:{hex};border-color:{hex};";
    }

    public static string ToHex(this RepairStatus status) => StatusColors[status].Hex;
}
