using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;
using WebAppExam.InventoryService.Infrastructure.Persistence;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class InventoryRepository : BaseRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(IMongoDatabase database, IMongoSessionProvider sessionProvider)
        : base(database, "Inventories", sessionProvider)
    {
    }

    public async Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.Eq(x => x.ProductId, productId);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).FirstOrDefaultAsync(cancellationToken);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Inventory> GetInventoryByCorreclationIdAsync(string correlationId, string productId, string warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.Eq(x => x.CorrelationId, correlationId)
           & Builders<Inventory>.Filter.Eq(x => x.ProductId, productId)
           & Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId);

        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).FirstOrDefaultAsync(cancellationToken);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Inventory>> GetInventoriesByCorreclationIdsAsync(List<string> correlationIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.CorrelationId, correlationIds);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).ToListAsync(cancellationToken);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Inventory> GetInventoryByProductIdAndWarehouseIdAsync(string productId, string warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter
        .Eq(x => x.ProductId, productId) &
        Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId);

        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).FirstOrDefaultAsync(cancellationToken);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetStockAsync(Ulid productId, Ulid warehouseId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(x => x.ProductId, productId.ToString()),
            Builders<Inventory>.Filter.Eq(x => x.WareHouseId, warehouseId.ToString())
        );

        var session = _sessionProvider.CurrentSession;
        Inventory stock;
        if (session != null)
            stock = await _collection.Find(session, filter).FirstOrDefaultAsync(cancellationToken);
        else
            stock = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

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

        var session = _sessionProvider.CurrentSession;
        UpdateResult result;
        if (session != null)
            result = await _collection.UpdateOneAsync(session, filter, update, cancellationToken: cancellationToken);
        else
            result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<List<Inventory>> GetInventoriesByIdsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.Id, ids);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).ToListAsync(cancellationToken);
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

        var session = _sessionProvider.CurrentSession;
        List<Inventory> inventories;
        if (session != null)
        {
            inventories = await _collection
                .Find(session, finalFilter)
                .Project<Inventory>(projection)
                .ToListAsync(cancellationToken);
        }
        else
        {
            inventories = await _collection
                .Find(finalFilter)
                .Project<Inventory>(projection)
                .ToListAsync(cancellationToken);
        }

        return inventories.ToDictionary(
            x => (Ulid.Parse(x.ProductId), Ulid.Parse(x.WareHouseId)),
            x => x.StockQuantity
        );
    }

    public async Task<List<Inventory>> GetByProductIdsAsync(List<string> productIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Inventory>.Filter.In(x => x.ProductId, productIds);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).ToListAsync(cancellationToken);

        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
}