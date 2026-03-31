using System;
using MediatR;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class UpdateInventoryCommand(string id) : IRequest<bool>
{
    public string Id { get; set; } = id;
    public int NewStockQuantity { get; set; }
}
