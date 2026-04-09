
using KafkaFlow.Producers;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Infrastructure.Constants;

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
        var orderReplyProducer = _producerAccessor.GetProducer(KafkaProducers.OrderReplyProducer);
        await orderReplyProducer.ProduceAsync(message.OrderId, message);
    }
}
