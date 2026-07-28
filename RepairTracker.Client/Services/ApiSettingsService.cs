using System.Net.Http.Json;
using RepairTracker.Models;
using RepairTracker.Services;

namespace RepairTracker.Client.Services;

public class ApiSettingsService(HttpClient http) : ISettingsService
{
    public async Task<AppSettings> GetAsync() =>
        await http.GetFromJsonAsync<AppSettings>("api/settings") ?? new AppSettings();

    public async Task SaveAsync(AppSettings settings)
    {
        var response = await http.PutAsJsonAsync("api/settings", settings);
        response.EnsureSuccessStatusCode();
    }
}
