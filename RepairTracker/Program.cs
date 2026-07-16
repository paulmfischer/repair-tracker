using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using RepairTracker.Components;
using RepairTracker.Data;
using RepairTracker.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var databaseName = builder.Configuration["MongoDB:Database"] ?? "RepairTracker";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<MongoDbContext>(sp => new MongoDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IReportService, ReportService>();

var dataProtectionPath = builder.Configuration["DataProtection:Path"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("RepairTracker");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

var externalUploadsPath = app.Configuration["Uploads:Path"];
if (!string.IsNullOrWhiteSpace(externalUploadsPath))
{
    Directory.CreateDirectory(externalUploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(externalUploadsPath),
        RequestPath = "/uploads"
    });
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/items/{id}/report.pdf", async (string id, IReportService reportService) =>
{
    var pdfBytes = await reportService.GenerateItemReportAsync(id);
    return pdfBytes is null
        ? Results.NotFound()
        : Results.File(pdfBytes, "application/pdf", $"report-{id}.pdf");
});

app.Run();
