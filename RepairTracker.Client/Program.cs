using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RepairTracker.Client.Services;
using RepairTracker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Plain client for ConnectivityService's own reachability probe, kept separate from the
// reporting-wrapped client below: ConnectivityReportingHandler needs ConnectivityService, so
// ConnectivityService can't depend on the client the handler wraps without a DI cycle.
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddScoped<ConnectivityService>();

builder.Services.AddTransient<ConnectivityReportingHandler>();
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<ConnectivityReportingHandler>();

builder.Services.AddScoped(sp => new ApiItemService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));
builder.Services.AddScoped(sp => new ApiSettingsService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));
builder.Services.AddScoped(sp => new ApiWikiArticleService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<IndexedDbStore>();
builder.Services.AddScoped<OutboxStore>();
builder.Services.AddScoped<IItemService, CachingItemService>();
builder.Services.AddScoped<ISettingsService, CachingSettingsService>();
builder.Services.AddScoped<OutboxSyncService>();

// Online-only for v1 — no IndexedDB caching/outbox decorator, unlike Items/Settings above.
builder.Services.AddScoped<IWikiArticleService>(sp => sp.GetRequiredService<ApiWikiArticleService>());

await builder.Build().RunAsync();
