using System;
using MediatR;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.WareHouse.Commands;

public class CreateWareHouseCommand : IRequest<WareHouseDTO>
{
    public string Address { get; set; }
    public string OwerName { get; set; }
    public string OwerEmail { get; set; }
    public string OwerPhone { get; set; }
}
