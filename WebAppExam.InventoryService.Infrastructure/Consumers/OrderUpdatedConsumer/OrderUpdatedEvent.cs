using System;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

public class OrderUpdatedEvent
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}
