
using KafkaFlow.Producers;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IProducerAccessor _producerAccessor;

    public OrderService(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task SendMessageReply(OrderReplyDTO message, bool isSuccess, string reason)
    {
        var orderReplyProducer = _producerAccessor.GetProducer("order-reply");
        await orderReplyProducer.ProduceAsync(message.OrderId, message);
    }
}
