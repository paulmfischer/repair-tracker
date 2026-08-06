using Microsoft.JSInterop;

namespace RepairTracker.Client.Services;

// navigator.onLine only reflects whether a network interface exists, not whether requests
// actually reach the server - and Chrome DevTools' offline emulation (both the Network
// throttle preset and the Application panel checkbox) doesn't reliably override it either,
// especially across a fresh page load. So the source of truth here is real request outcomes:
// an initial probe on startup, plus CachingItemService/CachingSettingsService/OutboxSyncService
// reporting every actual success/failure they see. The browser's online/offline events are kept
// only as a supplementary fast-path signal for real network drops.
public class ConnectivityService(IJSRuntime js, HttpClient http) : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ConnectivityService>? _dotNetRef;

    public bool IsOnline { get; private set; } = true;

    public event Action<bool>? OnlineStatusChanged;

    public async Task InitializeAsync()
    {
        _module = await js.InvokeAsync<IJSObjectReference>("import", "./js/connectivity.js");

        _dotNetRef = DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("registerConnectivityCallback", _dotNetRef);

        await ProbeAsync();
    }

    public async Task ProbeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await http.GetAsync("api/ping", cts.Token);
            SetOnline(response.IsSuccessStatusCode);
        }
        catch
        {
            SetOnline(false);
        }
    }

    public void ReportOnline() => SetOnline(true);

    public void ReportOffline() => SetOnline(false);

    private void SetOnline(bool isOnline)
    {
        if (IsOnline == isOnline)
        {
            return;
        }

        IsOnline = isOnline;
        OnlineStatusChanged?.Invoke(isOnline);
    }

    [JSInvokable]
    public void OnConnectivityChanged(bool isOnline) => SetOnline(isOnline);

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("unregisterConnectivityCallback");
            await _module.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
