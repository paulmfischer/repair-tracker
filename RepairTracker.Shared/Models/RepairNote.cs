using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepairTracker.Models;

public class RepairNote
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Text { get; set; } = string.Empty;
    public RepairStatus StatusAtTime { get; set; }
    public List<string> ImagePaths { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
