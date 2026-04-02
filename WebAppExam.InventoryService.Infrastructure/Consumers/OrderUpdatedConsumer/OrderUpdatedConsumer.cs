using System;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

public class OrderUpdatedConsumer : IMessageHandler<OrderUpdatedEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProducerAccessor _producerAccessor;

    public OrderUpdatedConsumer(
        IServiceProvider serviceProvider,
        IProducerAccessor producerAccessor)
    {
        _serviceProvider = serviceProvider;
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(IMessageContext context, OrderUpdatedEvent message)
    {
        bool isSuccess = true;
        string failReason = string.Empty;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

            foreach (var item in message.Items)
            {
                var stock = await inventoryRepo.GetStockAsync(Ulid.Parse(item.ProductId), Ulid.Parse(item.WareHouseId));
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
                    await inventoryRepo.DeductStockAsync(
                        Ulid.Parse(item.ProductId),
                        Ulid.Parse(item.WareHouseId),
                        item.Quantity);
                }
            }

            await SendMessageReply(message, isSuccess, failReason);
        }
        catch (Exception ex)
        {
            var reason = "Lỗi hệ thống nội bộ Inventory";
            await SendMessageReply(message, isSuccess, reason);
            Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
        }
    }

    private async Task SendMessageReply(OrderUpdatedEvent message, bool isSuccess, string reason)
    {
        var orderReplyProducer = _producerAccessor.GetProducer("order-updated-reply");
        await orderReplyProducer.ProduceAsync(message.OrderId, new
        {
            OrderId = message.OrderId,
            Status = isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
            Reason = reason,
            Data = message.Items.Select(item => new
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                WareHouseId = item.WareHouseId
            }).ToList(),
        });
    }
}
