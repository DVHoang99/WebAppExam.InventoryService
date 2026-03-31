using System;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class InventoryDTO
{
    public string Id { get; set; }
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}
