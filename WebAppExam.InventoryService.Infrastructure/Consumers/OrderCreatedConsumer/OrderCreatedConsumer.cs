using KafkaFlow;
using KafkaFlow.Producers;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedConsumer : IMessageHandler<OrderCreatedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IProducerAccessor _producerAccessor;

    public OrderCreatedConsumer(
        IInventoryService inventoryService,
        IProducerAccessor producerAccessor)
    {
        _inventoryService = inventoryService;
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(IMessageContext context, OrderCreatedEvent message)
    {
        bool isSuccess = true;
        string failReason = string.Empty;

        try
        {
            if (message.Items != null && message.Items.Any())
            {
                var requiredStocks = message.Items
                    .GroupBy(x => new { ProductId = Ulid.Parse(x.ProductId), WarehouseId = Ulid.Parse(x.WareHouseId) })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        g.Key.WarehouseId,
                        TotalRequired = g.Sum(x => x.Quantity),
                        RawProductIdStr = g.First().ProductId
                    })
                    .ToList();

                var input = message.Items.Select(x => new OrderItemDTO
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId
                }).ToList();

                (bool IsSuccess, string FailReason) stockDict = await _inventoryService.CheckAndDeductInventoryAsync(input);
                var orderReplyProducer = _producerAccessor.GetProducer("order-reply");
                await orderReplyProducer.ProduceAsync(message.OrderId, new
                {
                    OrderId = message.OrderId,
                    Status = isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
                    Reason = failReason
                });
            }
        }
        catch (Exception ex)
        {
            var orderReplyProducer = _producerAccessor.GetProducer("order-reply");
            await orderReplyProducer.ProduceAsync(message.OrderId, new
            {
                OrderId = message.OrderId,
                Status = OrderStatus.Failed,
                Reason = "Lỗi hệ thống nội bộ Inventory"
            });

            Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
        }
    }
}