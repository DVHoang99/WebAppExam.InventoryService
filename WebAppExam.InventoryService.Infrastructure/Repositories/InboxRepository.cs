using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class InboxRepository(IMongoDatabase database) : BaseRepository<InboxMessage>(database, "InboxMessages"), IInboxRepository
{
    public async Task<bool> ExistsAsync(string idempotencyId, string handlerName, CancellationToken cancellationToken = default)
    {
        var filter = Builders<InboxMessage>.Filter.And(
            Builders<InboxMessage>.Filter.Eq(x => x.Id, idempotencyId),
            Builders<InboxMessage>.Filter.Eq(x => x.HandlerName, handlerName)
        );

        return await _collection.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task CreateAsync(InboxMessage message, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(message, cancellationToken: cancellationToken);
    }
}
