using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer
{
    public class OrderDeletedEvent
    {
        public string OrderId { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
    }
}