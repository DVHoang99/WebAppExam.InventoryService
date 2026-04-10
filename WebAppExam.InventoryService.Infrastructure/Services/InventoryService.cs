using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Repositories;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IWareHouseRepository _wareHouseRepository;


    public InventoryService(IInventoryRepository inventoryRepo, IWareHouseRepository wareHouseRepository)
    {
        _inventoryRepo = inventoryRepo;
        _wareHouseRepository = wareHouseRepository;
    }

    public async Task<(bool IsSuccess, string FailReason)> CheckAndDeductInventoryAsync(
    IEnumerable<OrderItemDTO> items)
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

        // 1. Lấy stock - Truyền session để đảm bảo đọc dữ liệu nhất quán trong transaction
        var stockDict = await _inventoryRepo.GetStocksBulkAsync(keysToFetch);

        foreach (var req in requiredStocks)
        {
            var key = (req.ProductId, req.WarehouseId);

            if (!stockDict.TryGetValue(key, out var currentStock) || currentStock < req.TotalRequired)
            {
                return (false, $"Sản phẩm {req.RawProductIdStr} không đủ số lượng.");
            }
        }

        // 2. Trừ kho - Truyền session để việc trừ kho nằm trong transaction
        foreach (var item in items)
        {
            await _inventoryRepo.DeductStockAsync(
                Ulid.Parse(item.ProductId),
                Ulid.Parse(item.WareHouseId),
                item.Quantity);
        }

        return (true, string.Empty);
    }

    public async Task<List<GetBatchInventoryDTO>> GetBatchInventoryDTOsByIdsAnsyc(List<string> ids, CancellationToken cancellationToken)
    {
        var inventories = await _inventoryRepo.GetByProductIdsAsync(ids, cancellationToken);

        if (!inventories.Any())
        {
            return new List<GetBatchInventoryDTO>();
        }

        var wareHouseIds = inventories.Select(i => i.WareHouseId).Distinct().ToList();

        var wareHouses = await _wareHouseRepository.GetByIdsAync(wareHouseIds, cancellationToken);
        var wareHouseMap = wareHouses.ToDictionary(x => x.Id, x => x);
        return inventories.Select(i =>
        {
            var wareHouse = wareHouseMap.GetValueOrDefault(i.WareHouseId);
            var wareHouseDTO = wareHouse != null ? Application.WareHouse.DTOs.WareHouseDTO.FromResult(wareHouse) : null;
            return GetBatchInventoryDTO.Init(i.ProductId, i.StockQuantity, i.CorrelationId, i.WareHouseId, wareHouseDTO);
        }).ToList();
    }
}
