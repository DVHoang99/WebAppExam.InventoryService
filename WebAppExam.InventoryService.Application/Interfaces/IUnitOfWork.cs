
namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task StartTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}