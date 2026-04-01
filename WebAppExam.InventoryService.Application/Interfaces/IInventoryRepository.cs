using System;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IInventoryRepository : IBaseRepository<Inventory>
{
    Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<Inventory> GetInventoryByCorreclationIdAsync(string correlationId, string productId, string warehouseId, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetInventoriesByCorreclationIdsAsync(List<string> correlationIds, CancellationToken cancellationToken = default);
    Task<Inventory> GetInventoryByProductIdAndWarehouseIdAsync(string productId, string warehouseId, CancellationToken cancellationToken = default);
    Task<int> GetStockAsync(Ulid productId, Ulid warehouseId, CancellationToken cancellationToken = default);
    Task<bool> DeductStockAsync(Ulid productId, Ulid warehouseId, int quantity, CancellationToken cancellationToken = default);
}
