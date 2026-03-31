using System;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Domain.Entity;

public class Inventory : EntityBase, IEntity
{
    public string Id { get; set; }
    public string WareHouseId { get; set; }
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }

    public Inventory(string id, string productId, int stockQuantity, string name, string address)
    {
        Id = id;
        ProductId = productId;
        StockQuantity = stockQuantity;
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
    }

    public void OverrideStock(int quantity)
    {
        StockQuantity = quantity;
    }
}
