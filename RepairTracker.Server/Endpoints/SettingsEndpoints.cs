using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings");

        group.MapGet("/", async (ISettingsService settingsService) =>
            Results.Ok(await settingsService.GetAsync()));

        group.MapPut("/", async (AppSettings settings, ISettingsService settingsService) =>
        {
            await settingsService.SaveAsync(settings);
            return Results.NoContent();
        });
    }
}
