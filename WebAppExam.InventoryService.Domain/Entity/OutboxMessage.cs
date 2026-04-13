using System;
using MongoDB.Bson.Serialization.Attributes;
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Domain.Entity;

public class OutboxMessage : IEntity
{
    public string Id { get; set; }
    public string Topic { get; set; }
    public string Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; }

    private OutboxMessage(string id, string topic, string payload)
    {
        Id = id;
        Topic = topic;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
        IsProcessed = false;
    }
    
    public static OutboxMessage Init(string id, string topic, string payload)
    {
        return new OutboxMessage(id, topic, payload);
    }
}
