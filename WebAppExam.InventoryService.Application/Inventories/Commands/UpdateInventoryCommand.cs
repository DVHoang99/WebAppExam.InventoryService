using System;
using MediatR;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class UpdateInventoryCommand(string id, string wareHouseId, int newStockQuantity, string UpdateEventId) : IRequest<bool>
{
    public string Id { get; set; } = id;
    public int NewStockQuantity { get; set; } = newStockQuantity;
    public string WareHouseId { get; set; } = wareHouseId;
    public string UpdateEventId { get; set; } = UpdateEventId;
}
