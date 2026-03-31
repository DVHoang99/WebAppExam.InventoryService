using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity
{
    // Protected collection so derived classes can use it for custom queries
    protected readonly IMongoCollection<TEntity> _collection;

    public BaseRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<TEntity>(collectionName);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Insert a new document into the specified collection
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Replace the existing document matching the entity's Id
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public virtual async Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Find and return a single document by its Id
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
    public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        // Delete the document matching the provided Id
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }
}