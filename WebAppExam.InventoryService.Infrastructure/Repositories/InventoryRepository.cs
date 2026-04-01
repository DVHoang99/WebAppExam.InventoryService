using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
{
    // Pass the MongoDB database and the specific collection name to the base class
    public InventoryRepository(IMongoDatabase database)
        : base(database, "Inventories")
    {
    }

    public async Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.Eq(x => x.ProductId, productId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Inventory> GetInventoryByCorreclationIdAsync(string correlationId, string productId, string warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.Eq(x => x.CorrelationId, correlationId)
           & Builders<Inventory>.Filter.Eq(x => x.ProductId, productId)
           & Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Inventory>> GetInventoriesByCorreclationIdsAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.CorrelationId, correlationIds);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
    public async Task<Inventory> GetInventoryByProductIdAndWarehouseIdAsync(string productId, string warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.Eq(x => x.ProductId, productId) & Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetStockAsync(Ulid productId, Ulid warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(x => x.ProductId, productId.ToString()),
            Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId.ToString())
        );

        var stock = await _collection.Find(filter).FirstOrDefaultAsync();
        return stock?.StockQuantity ?? 0;
    }

    public async Task<bool> DeductStockAsync(Ulid productId, Ulid warehouseId, int quantity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(x => x.ProductId, productId.ToString()),
            Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId.ToString()),
            Builders<Inventory>.Filter.Gte(x => x.StockQuantity, quantity)
        );

        var update = Builders<Inventory>.Update.Inc(x => x.StockQuantity, -quantity);

        var result = await _collection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }
}