namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class KafkaSettings
{
    // Default Broker
    public const string DefaultBroker = "localhost:9092";

    // Topic Configuration
    public const int TopicPartitions = 3;
    public const short TopicReplicationFactor = 1;

    // Consumer Configuration
    public const int ConsumerWorkersCount = 2;
    public const int ConsumerBufferSize = 100;
}
