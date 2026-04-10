using System;
using MongoDB.Bson.Serialization.Attributes;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Domain.Entity;

public class InboxMessage : IEntity
{
    public string Id { get; set; }
    public string MessageId { get; set; }
    public string HandlerName { get; set; }
    public DateTime ProcessedAt { get; set; }

    private InboxMessage(string idempotencyId, string messageId, string handlerName, DateTime processedAt)
    {
        Id = idempotencyId;
        MessageId = messageId;
        HandlerName = handlerName;
        ProcessedAt = processedAt;
    }
    public static InboxMessage Init(string idempotencyId, string messageId, string handlerName)
    {
        return new InboxMessage(idempotencyId, messageId, handlerName, DateTime.UtcNow);
    }
}
