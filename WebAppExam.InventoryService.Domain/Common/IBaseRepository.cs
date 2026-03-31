using System;

namespace WebAppExam.InventoryService.Domain.Common;

public interface IBaseRepository<TEntity> where TEntity : IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}