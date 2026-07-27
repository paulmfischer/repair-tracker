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

builder.Services.AddScoped<IItemService, ApiItemService>();
builder.Services.AddScoped<ISettingsService, ApiSettingsService>();

await builder.Build().RunAsync();
