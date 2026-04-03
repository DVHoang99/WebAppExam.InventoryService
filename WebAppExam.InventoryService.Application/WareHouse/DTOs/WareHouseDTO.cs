using System;
using System.Net;

namespace WebAppExam.InventoryService.Application.WareHouse.DTOs;

public class WareHouseDTO
{
    public string? Id { get; set; }
    public string Address { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string OwnerPhone { get; set; }

    public static WareHouseDTO FromResult(Domain.Entity.WareHouse wareHouse)
    {
        if (wareHouse == null)
            return null;

        return new WareHouseDTO
        {
            Id = wareHouse.Id,
            Address = wareHouse.Address,
            OwnerName = wareHouse.OwnerName,
            OwnerEmail = wareHouse.OwnerEmail,
            OwnerPhone = wareHouse.OwnerPhone,
        };
    }

}
