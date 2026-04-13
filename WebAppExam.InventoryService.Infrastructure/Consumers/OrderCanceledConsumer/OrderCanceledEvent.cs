using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer
{
    public class OrderCanceledEvent
    {
        public string IdempotencyId { get; set; }
        public string OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
    }
}