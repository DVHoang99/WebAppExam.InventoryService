using System;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class InventoryDTO
{
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }
    public string CorrelationId { get; set; }
    public string WareHouseId { get; set; }

    public static InventoryDTO FromResult(Inventory inventory)
    {
        return new InventoryDTO()
        {
            ProductId = inventory.ProductId,
            StockQuantity = inventory.StockQuantity,
            WareHouseId = inventory.WareHouseId,
            CorrelationId = inventory.CorrelationId
        };
    }
}
