using System;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;
using WebAppExam.InventoryService.Infrastructure.Consumers;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

namespace WebAppExam.InventoryService.API.Common;

public class MessageTypeResolver : IMessageTypeResolver
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { nameof(OrderCreatedEvent), typeof(OrderCreatedEvent) },
        { nameof(OrderUpdatedEvent), typeof(OrderUpdatedEvent) },
        { nameof(OrderDeletedEvent), typeof(OrderDeletedEvent) },
        { nameof(OrderCanceledEvent), typeof(OrderCanceledEvent) },
    };
    public ValueTask<Type?> OnConsumeAsync(IMessageContext context)
    {
        var typeName = context.Headers.GetString("Message-Type");

        Console.WriteLine($"[Kafka-Resolver] Đang phiên dịch Type: '{typeName}'");

        if (typeName != null && _types.TryGetValue(typeName, out var type))
        {
            Console.WriteLine($"[Kafka-Resolver] Đã tìm thấy map cho: {type.Name}");
            return ValueTask.FromResult<Type?>(type);
        }

        Console.WriteLine($"[Kafka-Resolver] KHÔNG TÌM THẤY MAP CHO: '{typeName}'");
        return ValueTask.FromResult<Type?>(null);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        var type = context.Message.GetType();
        string alias = type.FullName!;

        // Nếu là 2 class này thì gán tên ngắn
        if (type == typeof(OrderCreatedEvent)) alias = "OrderCreated";
        else if (type == typeof(OrderUpdatedEvent)) alias = "OrderUpdated";

        // Ghi vào Header
        context.Headers.Add("Message-Type", System.Text.Encoding.UTF8.GetBytes(alias));

        return ValueTask.CompletedTask;
    }
}