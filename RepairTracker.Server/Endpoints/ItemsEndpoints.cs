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

        group.MapGet("/dashboard-stats", async (decimal feePercent, IItemService itemService) =>
            Results.Ok(await itemService.GetDashboardStatsAsync(feePercent)));

        group.MapGet("/{id}", async (string id, IItemService itemService) =>
        {
            var item = await itemService.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapPost("/", async (Item item, IItemService itemService) =>
        {
            await itemService.CreateAsync(item);
            return Results.Ok(item);
        });

        group.MapPut("/{id}", async (string id, Item item, IItemService itemService) =>
        {
            item.Id = id;
            await itemService.UpdateAsync(item);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (string id, IItemService itemService) =>
        {
            await itemService.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
