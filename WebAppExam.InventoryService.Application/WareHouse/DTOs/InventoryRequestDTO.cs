using System;

namespace WebAppExam.InventoryService.Application.WareHouse.DTOs;

public class InventoryRequestDTO
{
    public string WareHouseId { get; set; }
    public int Stock { get; set; }
    public string UpdateEventId { get; set; }
}
