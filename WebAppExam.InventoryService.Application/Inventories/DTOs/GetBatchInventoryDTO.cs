using System;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class GetBatchInventoryDTO
{
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }
    public string CorrelationId { get; set; }
    public string WareHouseId { get; set; }
    public WareHouseDTO WareHouse { get; set; }
}
