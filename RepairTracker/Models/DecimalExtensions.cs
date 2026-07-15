using MudBlazor;

namespace RepairTracker.Models;

public static class DecimalExtensions
{
    extension (decimal value)
    {
        public string ProfitColor() => value >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1;
    }
}