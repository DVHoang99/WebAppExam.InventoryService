using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
{
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

    public async Task<List<Inventory>> GetInventoriesByIdsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.Id, ids);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<(Ulid ProductId, Ulid WarehouseId), int>> GetStocksBulkAsync(List<(Ulid ProductId, Ulid WarehouseId)> keys, CancellationToken cancellationToken = default)
    {
        if (keys == null || !keys.Any())
        {
            return new Dictionary<(Ulid, Ulid), int>();
        }

        var filterBuilder = Builders<Inventory>.Filter;
        var filters = new List<FilterDefinition<Inventory>>();

        foreach (var key in keys)
        {
            var condition = filterBuilder.And(
                filterBuilder.Eq(x => x.ProductId, key.ProductId.ToString()),
                filterBuilder.Eq(x => x.WareHouseId, key.WarehouseId.ToString())
            );
            filters.Add(condition);
        }

        var finalFilter = filterBuilder.Or(filters);

        var projection = Builders<Inventory>.Projection
            .Include(x => x.ProductId)
            .Include(x => x.WareHouseId)
            .Include(x => x.StockQuantity);

        var inventories = await _collection
            .Find(finalFilter)
            .Project<Inventory>(projection)
            .ToListAsync(cancellationToken);

        return inventories.ToDictionary(
            x => (Ulid.Parse(x.ProductId), Ulid.Parse(x.WareHouseId)),
            x => x.StockQuantity
        );
    }

    public async Task<List<Inventory>> GetByProductIdsAsync(List<string> productIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.ProductId, productIds);
        var inventories = await _collection.Find(filter).ToListAsync(cancellationToken);
        return inventories;
    }
}