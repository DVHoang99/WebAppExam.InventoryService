using System;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Repositories;

public interface IInventoryRepository : IBaseRepository<Inventory>
{
    Task<Inventory> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<Inventory> GetInventoryByCorreclationIdAsync(string correlationId, string productId, string warehouseId, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetInventoriesByCorreclationIdsAsync(List<string> correlationIds, CancellationToken cancellationToken = default);
    Task<Inventory> GetInventoryByProductIdAndWarehouseIdAsync(string productId, string warehouseId, CancellationToken cancellationToken = default);
    Task<int> GetStockAsync(Ulid productId, Ulid warehouseId, CancellationToken cancellationToken = default);
    Task<bool> DeductStockAsync(Ulid productId, Ulid warehouseId, int quantity, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetInventoriesByIdsAsync(List<string> ids, CancellationToken cancellationToken = default);
    Task<Dictionary<(Ulid ProductId, Ulid WarehouseId), int>> GetStocksBulkAsync(List<(Ulid ProductId, Ulid WarehouseId)> keys, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetByProductIdsAsync(List<string> productIds, CancellationToken cancellationToken = default);
}
