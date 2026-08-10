namespace RepairTracker.Client.Services;

public enum OutboxOpType
{
    CreateItem,
    UpdateItem,
    DeleteItem,
    SaveSettings
}

public class OutboxOperation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public OutboxOpType OpType { get; set; }
    public string? TargetId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
