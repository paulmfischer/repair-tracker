using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class ItemsEndpoints
{
    public static void MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items").WithTags("Items");

        group.MapGet("/", async (IItemService itemService) =>
            Results.Ok(await itemService.GetAllAsync()))
            .WithSummary("List items")
            .WithDescription("Returns every tracked item.")
            .Produces<List<Item>>();

        group.MapGet("/dashboard-stats", async (decimal feePercent, decimal perOrderFee, IItemService itemService) =>
            Results.Ok(await itemService.GetDashboardStatsAsync(feePercent, perOrderFee)))
            .WithSummary("Get dashboard stats")
            .WithDescription("Aggregates counts and profit figures across all items, applying the given reseller fee percentage and flat per-order fee.")
            .Produces<DashboardStats>();

        group.MapGet("/{id}", async (string id, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching item {ItemId}", id);
            var item = await itemService.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
            .WithSummary("Get an item")
            .WithDescription("Returns a single item by id.")
            .Produces<Item>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (Item item, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating item {ItemId}", item.Id);
            await itemService.CreateAsync(item);
            return Results.Ok(item);
        })
            .WithSummary("Create an item")
            .WithDescription("Creates a new item. The id is minted by the caller; creating with an id that already exists upserts it (idempotent, so a replayed offline create doesn't duplicate).")
            .Produces<Item>();

        group.MapPut("/{id}", async (string id, Item item, IItemService itemService, ILogger<Program> logger) =>
        {
            item.Id = id;
            logger.LogInformation("Updating item {ItemId}", id);
            await itemService.UpdateAsync(item);
            return Results.NoContent();
        })
            .WithSummary("Update an item")
            .WithDescription("Replaces an existing item's fields, including its embedded repair notes.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{id}", async (string id, IItemService itemService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Deleting item {ItemId}", id);
            await itemService.DeleteAsync(id);
            return Results.NoContent();
        })
            .WithSummary("Delete an item")
            .WithDescription("Deletes an item by id. Does not remove its uploaded note images from disk.")
            .Produces(StatusCodes.Status204NoContent);
    }
}
