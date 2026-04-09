namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class KafkaConsumerGroups
{
    public const string InventoryOrderGroup = "inventory-order-group";
    public const string InventoryOrderDeletedGroup = "inventory-order-deleted-group";
    public const string InventoryOrderCanceledGroup = "inventory-order-canceled-group";
}
