namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class MessageConstants
{
    public const string MessageTypeHeader = "Message-Type";

    public const string OrderCreatedType = "OrderCreated";
    public const string OrderUpdatedType = "OrderUpdated";
    public const string OrderDeletedType = "OrderDeleted";
    public const string OrderCanceledType = "OrderCanceled";

    public const string ReplyCreatedType = "created";
    public const string ReplyUpdatedType = "updated";
    public const string ReplyDeletedType = "deleted";
    public const string ReplyCanceledType = "canceled";

    public const string InternalServerErrorMessage = "Internal Inventory System Error";
}
