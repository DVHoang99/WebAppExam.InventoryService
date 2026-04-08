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

    private WareHouseDTO(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        Id = id;
        Address = address;
        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
    }

    public static WareHouseDTO FromResult(Domain.Entity.WareHouse wareHouse)
    {
        return new WareHouseDTO(wareHouse.Id, wareHouse.Address, wareHouse.OwnerName, wareHouse.OwnerEmail, wareHouse.OwnerPhone);
    }
}
