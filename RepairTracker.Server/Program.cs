using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using RepairTracker.Data;
using RepairTracker.Server;
using RepairTracker.Server.Components;
using RepairTracker.Server.Endpoints;
using RepairTracker.Services;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithSpan();

    var seqUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        loggerConfig.WriteTo.Seq(seqUrl);
    }
});

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var databaseName = builder.Configuration["MongoDB:Database"] ?? "RepairTracker";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<MongoDbContext>(sp => new MongoDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<IItemService>(sp => sp.GetRequiredService<ItemService>());
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

app.UseSerilogRequestLogging(options =>
{
    // Static content (compiled JS/CSS/WASM assets, uploaded images, generated reports) has a file
    // extension on its last path segment; actual page/API routes in this app never do. Log those
    // at Verbose so they're dropped by the default Information minimum level instead of drowning
    // out real request logs, while still logging errors on them at Error regardless.
    options.GetLevel = (httpContext, elapsed, ex) => ex is not null
        ? LogEventLevel.Error
        : IsStaticAssetPath(httpContext.Request.Path)
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
});

static bool IsStaticAssetPath(PathString path)
{
    var lastSegment = path.Value?.Split('/').LastOrDefault();
    return !string.IsNullOrEmpty(lastSegment) && lastSegment.Contains('.');
}

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

// Cheap, DB-free reachability probe: DevTools' offline emulation doesn't reliably flip
// navigator.onLine (only real hardware disconnects reliably do), so ConnectivityService uses
// an actual round trip to this endpoint to determine connectivity instead.
app.MapGet("/api/ping", () => Results.Ok());

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
