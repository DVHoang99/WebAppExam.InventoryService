using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class InventoryRepository: BaseRepository<Inventory>, IInventoryRepository
{
    // Pass the MongoDB database and the specific collection name to the base class
    public InventoryRepository(IMongoDatabase database) 
        : base(database, "Inventories") 
    {
    }

    public async Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        // Custom query using the protected _collection from the base class
        var filter = Builders<Inventory>.Filter.Eq(x => x.ProductId, productId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}