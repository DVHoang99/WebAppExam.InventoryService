using System;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class OrderItemDTO
{
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
    public required string WareHouseId { get; init; }
}
