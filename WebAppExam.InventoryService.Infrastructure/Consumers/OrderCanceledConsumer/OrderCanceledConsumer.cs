using KafkaFlow;
using KafkaFlow.Producers;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer
{
    public class OrderCanceledConsumer : IMessageHandler<OrderCanceledEvent>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly IMongoClient _mongoClient;

        public OrderCanceledConsumer(IInventoryService inventoryService, IProducerAccessor producerAccessor, IOrderService orderService, IMongoClient mongoClient)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
            _mongoClient = mongoClient;
        }

        public async Task Handle(IMessageContext context, OrderCanceledEvent message)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                if (message.Items != null && message.Items.Any())
                {

                    var input = message.Items.Select(x => new OrderItemDTO
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        WareHouseId = x.WareHouseId
                    }).ToList();

                    (bool IsSuccess, string FailReason) stockDict = await _inventoryService.CheckAndDeductInventoryAsync(input);
                    await SendMessageReply(message, stockDict.IsSuccess, stockDict.FailReason, input);
                    await session.CommitTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                if (session.IsInTransaction) await session.AbortTransactionAsync();

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

        private async Task SendMessageReply(OrderCanceledEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
        {
            var orderReply = OrderReplyDTO.FromResult(
            message.OrderId,
            isSuccess ? message.Status : OrderStatus.Failed,
            reason, "canceled", [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

            await _orderService.SendMessageReply(orderReply, isSuccess, reason);
        }
    }
}