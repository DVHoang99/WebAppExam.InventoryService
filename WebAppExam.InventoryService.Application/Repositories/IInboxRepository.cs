using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Repositories;

public interface IInboxRepository : IBaseRepository<InboxMessage>
{
    Task<bool> ExistsAsync(string idempotencyId, string handlerName, CancellationToken cancellationToken = default);
    Task CreateAsync(InboxMessage message, CancellationToken cancellationToken = default);
}
