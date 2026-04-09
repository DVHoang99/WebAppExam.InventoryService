using System;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

public class OrderUpdatedConsumer : IMessageHandler<OrderUpdatedEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOrderService _orderService;

    public OrderUpdatedConsumer(
        IServiceProvider serviceProvider,
        IOrderService orderService)
    {
        _serviceProvider = serviceProvider;
        _orderService = orderService;
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
            await SendMessageReply(message, stockDict.IsSuccess, stockDict.FailReason, input);

        }
        catch (Exception ex)
        {
            var input = message.Items.Select(x => new OrderItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList();

            var reason = "Inventory service internal error";

            await SendMessageReply(message, false, reason, input);
            Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
        }
    }

    private async Task SendMessageReply(OrderUpdatedEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
    {
        var orderReply = OrderReplyDTO.FromResult(
                message.OrderId,
                isSuccess ? message.Status : OrderStatus.Failed,
                reason, "updated", [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

        await _orderService.SendMessageReply(orderReply, isSuccess, reason);
    }
}
