using Microsoft.JSInterop;

namespace RepairTracker.Client.Services;

public class ConnectivityService(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ConnectivityService>? _dotNetRef;

    public bool IsOnline { get; private set; } = true;

    public event Action<bool>? OnlineStatusChanged;

    public async Task InitializeAsync()
    {
        _module = await js.InvokeAsync<IJSObjectReference>("import", "./js/connectivity.js");
        IsOnline = await _module.InvokeAsync<bool>("isOnline");
        _dotNetRef = DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("registerConnectivityCallback", _dotNetRef);
    }

    [JSInvokable]
    public void OnConnectivityChanged(bool isOnline)
    {
        if (IsOnline == isOnline)
        {
            return;
        }

        IsOnline = isOnline;
        OnlineStatusChanged?.Invoke(isOnline);
    }

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
