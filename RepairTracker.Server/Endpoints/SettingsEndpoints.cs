using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGet("/", async (ISettingsService settingsService) =>
            Results.Ok(await settingsService.GetAsync()))
            .WithSummary("Get settings")
            .WithDescription("Returns the singleton app settings document, creating a default one on first read.")
            .Produces<AppSettings>();

        group.MapPut("/", async (AppSettings settings, ISettingsService settingsService) =>
        {
            await settingsService.SaveAsync(settings);
            return Results.NoContent();
        })
            .WithSummary("Save settings")
            .WithDescription("Upserts the singleton app settings document.")
            .Produces(StatusCodes.Status204NoContent);
    }
}
