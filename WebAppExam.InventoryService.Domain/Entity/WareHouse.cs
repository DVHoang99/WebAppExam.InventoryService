using System;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Exceptions;

namespace WebAppExam.InventoryService.Domain.Entity;

public class WareHouse : EntityBase, IEntity
{
    public string Id { get; private set; }
    public string Address { get; private set; }
    public string OwnerName { get; private set; }
    public string OwnerEmail { get; private set; }
    public string OwnerPhone { get; private set; }

    private WareHouse() { } // Required for ORM

    private WareHouse(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        Id = id;
        Address = address;
        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Method
    public static WareHouse Create(string id, string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new DomainException("Address is required");
        if (string.IsNullOrWhiteSpace(ownerName)) throw new DomainException("OwnerName is required");

        return new WareHouse(id, address, ownerName, ownerEmail, ownerPhone);
    }

    public void UpdateDetails(string address, string ownerName, string ownerEmail, string ownerPhone)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new DomainException("Address is required");
        if (string.IsNullOrWhiteSpace(ownerName)) throw new DomainException("OwnerName is required");

        Address = address;
        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
