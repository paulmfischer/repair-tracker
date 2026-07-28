using RepairTracker.Services;

namespace RepairTracker.Server.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/items/{id}/report.pdf", async (string id, IReportService reportService) =>
        {
            var pdfBytes = await reportService.GenerateItemReportAsync(id);
            return pdfBytes is null
                ? Results.NotFound()
                : Results.File(pdfBytes, "application/pdf", $"report-{id}.pdf");
        });
    }
}
