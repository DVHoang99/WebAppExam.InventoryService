using System;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.CalculateInventoryConsumer;

public class OutBoxMessageDTO
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string WareHouseId { get; set; }
    public string IdempotencyId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
