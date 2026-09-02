using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RepairTracker.Models;

public class PartLineItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public PartSource Source { get; set; } = PartSource.Bought;

    [BsonIgnore]
    public bool IsFree => Source is PartSource.Donor or PartSource.HadOnHand;

    [BsonIgnore]
    public decimal LineTotal => IsFree ? 0 : UnitCost * Quantity;
}
