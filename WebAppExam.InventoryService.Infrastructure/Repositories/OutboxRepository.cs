using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;
using WebAppExam.InventoryService.Infrastructure.Persistence;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class OutboxRepository : BaseRepository<OutboxMessage>, IOutboxRepository
{
    public OutboxRepository(IMongoDatabase database, IMongoSessionProvider sessionProvider) 
        : base(database, "OutboxMessages", sessionProvider)
    {
    }

    public async Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await AddAsync(message, cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.IsProcessed, false);
        var session = _sessionProvider.CurrentSession;
        
        if (session != null)
        {
            return await _collection.Find(session, filter)
                .SortBy(x => x.CreatedAt)
                .Limit(batchSize)
                .ToListAsync(cancellationToken);
        }

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

        var session = _sessionProvider.CurrentSession;
        if (session != null)
            await _collection.UpdateOneAsync(session, filter, update, cancellationToken: cancellationToken);
        else
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
