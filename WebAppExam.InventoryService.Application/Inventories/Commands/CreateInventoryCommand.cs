using System;
using MediatR;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class CreateInventoryCommand(InventoryDTO inventoryDTO) : IRequest<InventoryDTO>
{
    public string ProductId { get; set; } = inventoryDTO.ProductId;
    public int StockQuantity { get; set; } = inventoryDTO.StockQuantity;
    public string WareHouseId { get; set; } = inventoryDTO.WareHouseId;
    public string CorrelationId { get; set; } = inventoryDTO.CorrelationId;
}