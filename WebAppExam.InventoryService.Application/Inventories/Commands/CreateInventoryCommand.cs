using System;
using MediatR;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class CreateInventoryCommand : IRequest<string>
{
    public string Id { get; set; }
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}