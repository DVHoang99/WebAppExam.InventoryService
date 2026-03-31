using System;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Domain.Entity;

public class WareHouse : EntityBase, IEntity
{
    public string Id { get; set; }
    public string Address { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string OwnerPhone { get; set; }
    public WareHouse(string id, string address, string owerName, string owerEmail, string owerPhone)
    {
        Id = id;
        Address = address;
        OwnerName = owerName;
        OwnerEmail = owerEmail;
        OwnerPhone = owerPhone;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string address, string owerName, string owerEmail, string owerPhone)
    {
        Address = address;
        Address = address;
        OwnerName = owerName;
        OwnerEmail = owerEmail;
        OwnerPhone = owerPhone;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
