using System;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class OrderItemDTO
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string WareHouseId { get; set; }
}
