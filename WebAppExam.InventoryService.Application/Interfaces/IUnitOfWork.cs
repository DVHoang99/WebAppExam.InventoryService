using System;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}