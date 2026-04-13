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

    private InventoryDTO(string productId, int stockQuantity, string correlationId, string wareHouseId)
    {
        ProductId = productId;
        StockQuantity = stockQuantity;
        CorrelationId = correlationId;
        WareHouseId = wareHouseId;
    }

    public static InventoryDTO FromResult(string productId, int stockQuantity, string correlationId, string wareHouseId)
    {
        return new InventoryDTO(productId, stockQuantity, correlationId, wareHouseId);
    }
}
