using System;
using MediatR;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.Queries;

public class GetInventoryByIdQuery(string id) : IRequest<InventoryDTO>
{
    public string Id { get; set; } = id;
}
