using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using RepairTracker.Data;
using RepairTracker.Server;
using RepairTracker.Server.Components;
using RepairTracker.Server.Endpoints;
using RepairTracker.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

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
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

// Served from outside wwwroot (see UploadsPath), and always via plain UseStaticFiles rather
// than MapStaticAssets, since these files are written at runtime and have no build-time manifest entry.
var uploadsRoot = UploadsPath.GetRoot(app.Environment, app.Configuration);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RepairTracker.Client._Imports).Assembly);

app.MapItemsEndpoints();
app.MapSettingsEndpoints();
app.MapImagesEndpoints();
app.MapReportEndpoints();

app.Run();
