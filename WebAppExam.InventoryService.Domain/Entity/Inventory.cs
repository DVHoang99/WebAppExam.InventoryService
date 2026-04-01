using System;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Domain.Entity;

public class Inventory : EntityBase, IEntity
{
    public string Id { get; set; }
    public string WareHouseId { get; set; }
    public string ProductId { get; set; }
    public int StockQuantity { get; set; }
    public string CorrelationId { get; set; }
    public List<string> ProcessedUpdateIds { get; set; } = new List<string>();


    public Inventory(string id, string productId, int stockQuantity, string correlationId, string warehouseId)
    {
        Id = id;
        ProductId = productId;
        StockQuantity = stockQuantity;
        CreatedAt = DateTime.UtcNow;
        CorrelationId = correlationId;
        WareHouseId = warehouseId;
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
