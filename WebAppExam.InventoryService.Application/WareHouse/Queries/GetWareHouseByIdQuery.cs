using System;
using MediatR;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.WareHouse.Queries;

public class GetWareHouseByIdQuery(string id) : IRequest<WareHouseDTO>
{
    public string Id { get; set; } = id;
}
