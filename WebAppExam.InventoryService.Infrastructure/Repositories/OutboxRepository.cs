using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class OutboxRepository(IMongoDatabase database) : BaseRepository<OutboxMessage>(database, "OutboxMessages"), IOutboxRepository
{
    public async Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(message, cancellationToken: cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.IsProcessed, false);
        return await _collection.Find(filter)
            .SortBy(x => x.CreatedAt)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Id, id);
        var update = Builders<OutboxMessage>.Update
            .Set(x => x.IsProcessed, true)
            .Set(x => x.ProcessedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
