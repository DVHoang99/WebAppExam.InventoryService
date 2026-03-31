using System;
using MediatR;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class DeleteInventoryCommand(string id) : IRequest<bool>
{
    public string Id { get; set; } = id;
}