using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class ItemsEndpoints
{
    public static void MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items");

        group.MapGet("/", async (IItemService itemService) =>
            Results.Ok(await itemService.GetAllAsync()));

        group.MapGet("/dashboard-stats", async (decimal feePercent, decimal perOrderFee, IItemService itemService) =>
            Results.Ok(await itemService.GetDashboardStatsAsync(feePercent, perOrderFee)));

        group.MapGet("/{id}", async (string id, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching item {ItemId}", id);
            var item = await itemService.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapPost("/", async (Item item, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating item {ItemId}", item.Id);
            await itemService.CreateAsync(item);
            return Results.Ok(item);
        });

        group.MapPut("/{id}", async (string id, Item item, IItemService itemService, ILogger<Program> logger) =>
        {
            item.Id = id;
            logger.LogInformation("Updating item {ItemId}", id);
            await itemService.UpdateAsync(item);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (string id, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Deleting item {ItemId}", id);
            await itemService.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
