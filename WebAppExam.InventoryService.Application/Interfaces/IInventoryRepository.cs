using System;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IInventoryRepository : IBaseRepository<Inventory>
{
    Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
}
