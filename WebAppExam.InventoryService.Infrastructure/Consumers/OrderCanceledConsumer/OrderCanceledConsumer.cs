using KafkaFlow;
using KafkaFlow.Producers;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer
{
    public class OrderCanceledConsumer : IMessageHandler<OrderCanceledEvent>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProducerAccessor _producerAccessor;


        public OrderCanceledConsumer(IInventoryService inventoryService, IProducerAccessor producerAccessor)
        {
            _inventoryService = inventoryService;
            _producerAccessor = producerAccessor;
        }

        public async Task Handle(IMessageContext context, OrderCanceledEvent message)
        {
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
                    var orderReplyProducer = _producerAccessor.GetProducer("order-canceled-reply");
                    await orderReplyProducer.ProduceAsync(message.OrderId, new
                    {
                        OrderId = message.OrderId,
                        Status = stockDict.IsSuccess ? OrderStatus.Cancel : OrderStatus.Failed,
                        Reason = stockDict.FailReason
                    });
                }
            }
            catch (Exception ex)
            {
                var orderReplyProducer = _producerAccessor.GetProducer("order-canceled-reply");
                await orderReplyProducer.ProduceAsync(message.OrderId, new
                {
                    OrderId = message.OrderId,
                    Status = OrderStatus.Failed,
                    Reason = "Inventory service internal error"
                });

                Console.WriteLine($"[Error] - Inventory Handler: {ex.Message}");
            }
        }
    }
}