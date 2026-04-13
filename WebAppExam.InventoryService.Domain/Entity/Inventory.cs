using System;
using System.Collections.Generic;
using WebAppExam.InventoryService.Domain.Common;
using WebAppExam.InventoryService.Domain.Exceptions;

namespace WebAppExam.InventoryService.Domain.Entity;

public class Inventory : EntityBase, IEntity
{
    public string Id { get; private set; }
    public string WareHouseId { get; private set; }
    public string ProductId { get; private set; }
    public int StockQuantity { get; private set; }
    public string CorrelationId { get; private set; }
    public List<string> ProcessedUpdateIds { get; private set; } = new List<string>();

    private Inventory() { } // Required for ORM/Deserialization

    private Inventory(string id, string productId, int stockQuantity, string correlationId, string warehouseId)
    {
        Id = id;
        ProductId = productId;
        StockQuantity = stockQuantity;
        CorrelationId = correlationId;
        WareHouseId = warehouseId;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Method
    public static Inventory Create(string id, string productId, int stockQuantity, string correlationId, string warehouseId)
    {
        if (string.IsNullOrWhiteSpace(productId)) throw new DomainException("ProductId is required");
        if (string.IsNullOrWhiteSpace(warehouseId)) throw new DomainException("WareHouseId is required");
        if (stockQuantity < 0) throw new DomainException("Initial stock cannot be negative");

        return new Inventory(id, productId, stockQuantity, correlationId, warehouseId);
    }

    public void DeductStock(int quantity)
    {
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity to restock must be positive");

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void OverrideStock(int quantity)
    {
        if (quantity < 0) throw new DomainException("Stock quantity cannot be negative");

        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddProcessedUpdate(string updateEventId)
    {
        if (!ProcessedUpdateIds.Contains(updateEventId))
        {
            ProcessedUpdateIds.Add(updateEventId);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
