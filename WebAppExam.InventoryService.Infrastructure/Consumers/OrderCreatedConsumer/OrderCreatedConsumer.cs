using KafkaFlow;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedConsumer : IMessageHandler<OrderCreatedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;


    public OrderCreatedConsumer(
        IInventoryService inventoryService,
        IOrderService orderService)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
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

                await SendMessageReply(message, isSuccess, failReason, input);
            }
        }
        catch (Exception ex)
        {
            var input = message.Items.Select(x => new OrderItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList();

            var reason = "Lỗi hệ thống nội bộ Inventory";

            await SendMessageReply(message, false, reason, input);

            Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
        }
    }
    private async Task SendMessageReply(OrderCreatedEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
    {
        var orderReply = OrderReplyDTO.FromResult(
               message.OrderId,
               isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
               reason, "created", [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

        await _orderService.SendMessageReply(orderReply, isSuccess, reason);
    }
}