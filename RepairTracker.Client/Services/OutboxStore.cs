using System.Text.Json;
using RepairTracker.Models;

namespace RepairTracker.Client.Services;

public class OutboxStore(IndexedDbStore db)
{
    private const string StoreName = "outbox";

    public async Task<List<OutboxOperation>> GetPendingAsync()
    {
        var all = await db.GetAllAsync<OutboxOperation>(StoreName);
        return all.OrderBy(o => o.CreatedAt).ToList();
    }

    public Task RemoveAsync(string id) => db.RemoveAsync(StoreName, id);

    public Task EnqueueCreateAsync(Item item) => ReplaceForTargetAsync(OutboxOpType.CreateItem, item.Id, item);

    public Task EnqueueUpdateAsync(Item item) => ReplaceForTargetAsync(OutboxOpType.UpdateItem, item.Id, item);

    public Task EnqueueSaveSettingsAsync(AppSettings settings) =>
        ReplaceForTargetAsync(OutboxOpType.SaveSettings, targetId: null, settings);

    public async Task EnqueueDeleteAsync(string itemId)
    {
        var pending = await GetPendingAsync();

        // Created and deleted entirely while offline - nothing ever needs to reach the server.
        var pendingCreate = pending.FirstOrDefault(o => o.OpType == OutboxOpType.CreateItem && o.TargetId == itemId);
        if (pendingCreate is not null)
        {
            await RemoveAsync(pendingCreate.Id);
            foreach (var stale in pending.Where(o => o.OpType == OutboxOpType.UpdateItem && o.TargetId == itemId))
            {
                await RemoveAsync(stale.Id);
            }
            return;
        }

        // Any queued edits are moot once the item is being deleted.
        foreach (var stale in pending.Where(o => o.OpType == OutboxOpType.UpdateItem && o.TargetId == itemId))
        {
            await RemoveAsync(stale.Id);
        }

        await ReplaceForTargetAsync(OutboxOpType.DeleteItem, itemId, payload: null);
    }

    private async Task ReplaceForTargetAsync(OutboxOpType opType, string? targetId, object? payload)
    {
        var pending = await GetPendingAsync();
        var operation = pending.FirstOrDefault(o => o.OpType == opType && o.TargetId == targetId)
            ?? new OutboxOperation { OpType = opType, TargetId = targetId };

        operation.PayloadJson = payload is null ? string.Empty : JsonSerializer.Serialize(payload);
        operation.CreatedAt = DateTime.UtcNow;

        await db.PutAsync(StoreName, operation);
    }
}
