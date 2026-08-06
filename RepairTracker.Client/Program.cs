using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RepairTracker.Client.Services;
using RepairTracker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<ApiItemService>();
builder.Services.AddScoped<ApiSettingsService>();
builder.Services.AddScoped<IndexedDbStore>();
builder.Services.AddScoped<OutboxStore>();
builder.Services.AddScoped<IItemService, CachingItemService>();
builder.Services.AddScoped<ISettingsService, CachingSettingsService>();
builder.Services.AddScoped<ConnectivityService>();
builder.Services.AddScoped<OutboxSyncService>();

await builder.Build().RunAsync();
