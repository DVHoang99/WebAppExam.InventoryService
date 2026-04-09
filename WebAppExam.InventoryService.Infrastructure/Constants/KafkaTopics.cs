namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class KafkaTopics
{
    // Topics
    public const string OrderEventTopic = "order-topic1";
    public const string OrderDeletedTopic = "order-deleted-topic";
    public const string OrderCanceledTopic = "order-canceled-topic";

    // Reply Topics
    public const string OrderReplyTopic = "order-reply-topic";
    public const string OrderUpdatedReplyTopic = "order-updated-reply-topic";
    public const string OrderCanceledReplyTopic = "order-canceled-reply-topic";
}
