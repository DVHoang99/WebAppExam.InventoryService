using System;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedEvent
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}

public class OrderItemEvent
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string WareHouseId { get; set; }
}
