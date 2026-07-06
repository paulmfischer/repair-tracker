using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepairTracker.Models;

public class AppSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public decimal ResellerFeePercentage { get; set; } = 9m;
}
