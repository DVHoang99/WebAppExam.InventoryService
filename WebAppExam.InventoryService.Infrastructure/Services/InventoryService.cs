using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Exceptions;

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
    OrderItemDTO item)
    {
        if (item == null)
            return (false, "Invalid order item.");

        try
        {
            // 1. Process items
            var inventory = await _inventoryRepo.GetInventoryByProductIdAndWarehouseIdAsync(item.ProductId, item.WareHouseId);

            if (inventory == null)
            {
                return (false, $"Inventory not found for product {item.ProductId} in warehouse {item.WareHouseId}");
            }

            // Call Domain logic
            inventory.DeductStock(item.Quantity);

            // Persist changes
            await _inventoryRepo.UpdateAsync(inventory);

            return (true, string.Empty);
        }
        catch (InsufficientStockException ex)
        {
            return (false, ex.Message);
        }
        catch (DomainException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, "An unexpected error occurred during inventory deduction.");
        }
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
