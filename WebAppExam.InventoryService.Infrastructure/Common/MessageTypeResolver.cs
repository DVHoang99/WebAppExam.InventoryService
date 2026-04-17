using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;
using WebAppExam.InventoryService.Infrastructure.Consumers;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.Infrastructure.Common;

public class MessageTypeResolver : IMessageTypeResolver
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { nameof(OrderCreatedEvent), typeof(OrderCreatedEvent) },
        { nameof(OrderUpdatedEvent), typeof(OrderUpdatedEvent) },
        { nameof(OrderDeletedEvent), typeof(OrderDeletedEvent) },
        { nameof(OrderCanceledEvent), typeof(OrderCanceledEvent) },
        { nameof(OutboxPointer), typeof(OutboxPointer) },
    };
    public ValueTask<Type?> OnConsumeAsync(IMessageContext context)
    {
        var typeName = context.Headers.GetString(MessageConstants.MessageTypeHeader);

        Console.WriteLine($"[Kafka-Resolver] Resolving Type: '{typeName}'");

        if (typeName != null && _types.TryGetValue(typeName, out var type))
        {
            Console.WriteLine($"[Kafka-Resolver] Found map for: {type.Name}");
            return ValueTask.FromResult<Type?>(type);
        }

        Console.WriteLine($"[Kafka-Resolver] MAP NOT FOUND FOR: '{typeName}'");
        return ValueTask.FromResult<Type?>(null);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        var type = context.Message.GetType();
        string alias = type.FullName!;

        // If it is one of these 2 classes, assign a short name
        if (type == typeof(OrderCreatedEvent)) alias = MessageConstants.OrderCreatedType;
        else if (type == typeof(OrderUpdatedEvent)) alias = MessageConstants.OrderUpdatedType;
        else if (type == typeof(OrderCanceledEvent)) alias = MessageConstants.OrderCanceledType;
        else if (type == typeof(OrderDeletedEvent)) alias = MessageConstants.OrderDeletedType;
        else if (type == typeof(OutboxPointer)) alias = MessageConstants.OutboxPointerType;


        // Write to Header
        context.Headers.Add(MessageConstants.MessageTypeHeader, System.Text.Encoding.UTF8.GetBytes(alias));

        return ValueTask.CompletedTask;
    }
}
