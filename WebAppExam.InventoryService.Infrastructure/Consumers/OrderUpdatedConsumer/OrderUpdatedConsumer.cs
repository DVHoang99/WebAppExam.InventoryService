using System;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
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
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var input = message.Items.Select(x => new OrderItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList();

            (bool IsSuccess, string FailReason) stockDict = await inventoryRepo.CheckAndDeductInventoryAsync(input);

            await SendMessageReply(message, stockDict.IsSuccess, stockDict.FailReason);
        }
        catch (Exception ex)
        {
            var reason = "Inventory service internal error";
            await SendMessageReply(message, false, reason);
            Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
        }
    }

    private async Task SendMessageReply(OrderUpdatedEvent message, bool isSuccess, string reason)
    {
        var orderReplyProducer = _producerAccessor.GetProducer("order-updated-reply");
        await orderReplyProducer.ProduceAsync(message.OrderId, new
        {
            OrderId = message.OrderId,
            Status = isSuccess ? message.Status : OrderStatus.Failed,
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
