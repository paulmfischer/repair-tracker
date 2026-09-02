using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/items/{id}/report.pdf", async (string id, IReportService reportService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Generating report for item {ItemId}", id);
            var pdfBytes = await reportService.GenerateItemReportAsync(id);
            if (pdfBytes is null)
            {
                logger.LogWarning("Report generation failed: item {ItemId} not found", id);
                return Results.NotFound();
            }
            logger.LogInformation("Report generated for item {ItemId}", id);
            return Results.File(pdfBytes, "application/pdf", $"report-{id}.pdf");
        })
            .WithTags("Reports")
            .WithSummary("Get an item's PDF report")
            .WithDescription("Generates and returns a PDF summary of an item's repair history.")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status404NotFound);
    }
}
