using System;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;

    public InventoryService(IInventoryRepository inventoryRepo)
    {
        _inventoryRepo = inventoryRepo;
    }

    public async Task<(bool IsSuccess, string FailReason)> CheckAndDeductInventoryAsync(IEnumerable<OrderItemDTO> items)
    {
        if (items == null || !items.Any())
            return (true, string.Empty);

        var requiredStocks = items
        .GroupBy(x => new { ProductId = Ulid.Parse(x.ProductId), WarehouseId = Ulid.Parse(x.WareHouseId) })
        .Select(g => new
        {
            g.Key.ProductId,
            g.Key.WarehouseId,
            TotalRequired = g.Sum(x => x.Quantity),
            RawProductIdStr = g.First().ProductId
        })
        .ToList();

        var keysToFetch = requiredStocks.Select(x => (x.ProductId, x.WarehouseId)).ToList();

        var stockDict = await _inventoryRepo.GetStocksBulkAsync(keysToFetch);

        foreach (var req in requiredStocks)
        {
            var key = (req.ProductId, req.WarehouseId);

            if (!stockDict.TryGetValue(key, out var currentStock) || currentStock < req.TotalRequired)
            {
                return (false, $"Sản phẩm {req.RawProductIdStr} không đủ số lượng.");
            }
        }

        foreach (var item in items)
        {
            await _inventoryRepo.DeductStockAsync(
                Ulid.Parse(item.ProductId),
                Ulid.Parse(item.WareHouseId),
                item.Quantity);
        }

        return (true, string.Empty);
    }
}
