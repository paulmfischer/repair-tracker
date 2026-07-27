using MongoDB.Driver;
using RepairTracker.Models;

namespace RepairTracker.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Item> Items => _database.GetCollection<Item>("items");
    public IMongoCollection<AppSettings> Settings => _database.GetCollection<AppSettings>("settings");
}
