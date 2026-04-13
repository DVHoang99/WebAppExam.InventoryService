using System;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

public class OrderUpdatedEvent
{
    public string IdempotencyId { get; set; }
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}
