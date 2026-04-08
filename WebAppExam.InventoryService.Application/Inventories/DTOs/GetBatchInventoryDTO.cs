using System;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class GetBatchInventoryDTO
{
    public string ProductId { get; init; }
    public int StockQuantity { get; init; }
    public string CorrelationId { get; init; }
    public string WareHouseId { get; init; }
    public WareHouseDTO? WareHouse { get; init; }

    private GetBatchInventoryDTO(string productId, int stockQuantity, string correlationId, string wareHouseId, WareHouseDTO? wareHouse)
    {
        ProductId = productId;
        StockQuantity = stockQuantity;
        CorrelationId = correlationId;
        WareHouseId = wareHouseId;
        WareHouse = wareHouse;
    }

    public static GetBatchInventoryDTO Init(string productId, int stockQuantity, string correlationId, string wareHouseId, WareHouseDTO? wareHouse)
    {
        return new GetBatchInventoryDTO(productId, stockQuantity, correlationId, wareHouseId, wareHouse);
    }
}
