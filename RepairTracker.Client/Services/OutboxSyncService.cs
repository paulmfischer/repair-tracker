using System.Text.Json;
using MudBlazor;
using RepairTracker.Models;

namespace RepairTracker.Client.Services;

// Replays queued offline writes against the real API. Depends on the raw Api*Service classes
// (not the Caching* decorators) so a replayed write can't recursively re-enqueue itself.
public class OutboxSyncService(
    ApiItemService itemApi,
    ApiSettingsService settingsApi,
    OutboxStore outbox,
    ConnectivityService connectivity,
    ISnackbar snackbar)
{
    private bool _syncing;
    private bool _subscribed;

    public void Initialize()
    {
        if (!_subscribed)
        {
            connectivity.OnlineStatusChanged += OnConnectivityChanged;
            _subscribed = true;
        }

        if (connectivity.IsOnline)
        {
            _ = SyncAsync();
        }
    }

    private void OnConnectivityChanged(bool isOnline)
    {
        if (isOnline)
        {
            _ = SyncAsync();
        }
    }

    public async Task SyncAsync()
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            var pending = await outbox.GetPendingAsync();
            var syncedCount = 0;

            foreach (var operation in pending)
            {
                try
                {
                    await ApplyAsync(operation);
                    await outbox.RemoveAsync(operation.Id);
                    syncedCount++;
                }
                catch (HttpRequestException ex) when (ex.StatusCode is null)
                {
                    // Genuine connectivity failure - stop and retry the whole remaining queue in order next time.
                    break;
                }
                catch (HttpRequestException)
                {
                    // The server was reached but rejected this specific change (e.g. a validation
                    // error) - retrying won't help, so drop it rather than blocking everything
                    // queued after it, and let the user know it needs to be redone.
                    await outbox.RemoveAsync(operation.Id);
                    snackbar.Add("An offline change couldn't be saved and was discarded - please redo it.", Severity.Error);
                }
            }

            if (syncedCount > 0)
            {
                snackbar.Add(syncedCount == 1 ? "Synced 1 offline change" : $"Synced {syncedCount} offline changes", Severity.Success);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    private Task ApplyAsync(OutboxOperation operation) => operation.OpType switch
    {
        OutboxOpType.CreateItem => itemApi.CreateAsync(Deserialize<Item>(operation)),
        OutboxOpType.UpdateItem => itemApi.UpdateAsync(Deserialize<Item>(operation)),
        OutboxOpType.DeleteItem => itemApi.DeleteAsync(operation.TargetId!),
        OutboxOpType.SaveSettings => settingsApi.SaveAsync(Deserialize<AppSettings>(operation)),
        _ => Task.CompletedTask
    };

    private static T Deserialize<T>(OutboxOperation operation) =>
        JsonSerializer.Deserialize<T>(operation.PayloadJson)
        ?? throw new InvalidOperationException($"Outbox payload for {operation.OpType} {operation.Id} failed to deserialize.");
}
