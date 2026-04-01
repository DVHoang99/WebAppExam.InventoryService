using System;
using MediatR;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class DeleteInventoryCommand(string id, string wareHouseId) : IRequest<bool>
{
    public string Id { get; set; } = id;
    public string WareHouseId { get; set; } = wareHouseId;

}