using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Infrastructure.Persistence;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : IEntity
{
    protected readonly IMongoCollection<TEntity> _collection;
    protected readonly IMongoSessionProvider _sessionProvider;

    public BaseRepository(IMongoDatabase database, string collectionName, IMongoSessionProvider sessionProvider)
    {
        _collection = database.GetCollection<TEntity>(collectionName);
        _sessionProvider = sessionProvider;
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            await _collection.InsertOneAsync(session, entity, cancellationToken: cancellationToken);
        else
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            await _collection.ReplaceOneAsync(session, filter, entity, cancellationToken: cancellationToken);
        else
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public virtual async Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).FirstOrDefaultAsync(cancellationToken);
        
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            await _collection.DeleteOneAsync(session, filter, cancellationToken: cancellationToken);
        else
            await _collection.DeleteOneAsync(filter, cancellationToken);
    }
}