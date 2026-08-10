using Microsoft.JSInterop;

namespace RepairTracker.Client.Services;

public class IndexedDbStore(IJSRuntime js)
{
    private IJSObjectReference? _module;

    private async Task<IJSObjectReference> GetModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/offlineDb.js");

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<List<T>>("getAll", storeName);
    }

    public async Task<T?> GetAsync<T>(string storeName, string key)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<T?>("get", storeName, key);
    }

    public async Task PutAsync<T>(string storeName, T value)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("put", storeName, value);
    }

    public async Task RemoveAsync(string storeName, string key)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("remove", storeName, key);
    }
}
