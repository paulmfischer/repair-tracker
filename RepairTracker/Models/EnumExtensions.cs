using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RepairTracker.Models;

public static class EnumExtensions
{
    public static string ToDisplayString(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }
}
