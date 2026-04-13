using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Repositories;

public interface IOutboxRepository
{
    Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string id, CancellationToken cancellationToken = default);
}
