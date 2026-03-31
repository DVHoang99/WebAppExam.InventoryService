using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public MongoDbContext(string connectionString, string databaseName)
    {
        // Initialize MongoDB client and get database
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    public IMongoCollection<Inventory> Inventories
        => _database.GetCollection<Inventory>("Inventories");
    public IMongoCollection<WareHouse> WareHouses
        => _database.GetCollection<WareHouse>("WareHouses");

}
