using System;
using KafkaFlow;
using KafkaFlow.Producers;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedConsumer : IMessageHandler<OrderCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IProducerAccessor _producerAccessor;

    public OrderCreatedConsumer(
        IInventoryRepository inventoryRepo,
        IProducerAccessor producerAccessor)
    {
        _inventoryRepo = inventoryRepo;
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(IMessageContext context, OrderCreatedEvent message)
    {
        bool isSuccess = true;
        string failReason = string.Empty;

        try
        {
            foreach (var item in message.Items)
            {
                var stock = await _inventoryRepo.GetStockAsync(Ulid.Parse(item.ProductId), Ulid.Parse(item.WareHouseId));
                if (stock < item.Quantity)
                {
                    isSuccess = false;
                    failReason = $"Sản phẩm {item.ProductId} không đủ số lượng.";
                    break; 
                }
            }

            if (isSuccess)
            {
                foreach (var item in message.Items)
                {
                    await _inventoryRepo.DeductStockAsync(
                        Ulid.Parse(item.ProductId),
                        Ulid.Parse(item.WareHouseId),
                        item.Quantity);
                }
            }
            var orderReplyProducer = _producerAccessor.GetProducer("order-reply");
            await orderReplyProducer.ProduceAsync(message.OrderId, new
            {
                OrderId = message.OrderId,
                Status = isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
                Reason = failReason
            });
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