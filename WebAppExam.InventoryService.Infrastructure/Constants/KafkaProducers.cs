namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class KafkaProducers
{
    public const string OrderReplyProducer = "order-reply";
    public const string OrderUpdatedReplyProducer = "order-updated-reply";
    public const string OrderCanceledReplyProducer = "order-canceled-reply";
}
