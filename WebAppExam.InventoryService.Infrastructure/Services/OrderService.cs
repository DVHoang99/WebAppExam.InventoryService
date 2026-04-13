using System;
using System.Text.Json;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Microsoft.Extensions.Logging;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOutboxRepository outboxRepository,
        IProducerAccessor producerAccessor,
        ILogger<OrderService> logger)
    {
        _outboxRepository = outboxRepository;
        _producerAccessor = producerAccessor;
        _logger = logger;
    }

    public async Task SendMessageReply(OrderReplyDTO message, bool isSuccess, string reason)
    {
        string producerName = KafkaProducers.OrderReplyProducer;

        var outboxMessage = OutboxMessage.Init(
            message.IdenpotencyId,
            producerName,
            JsonSerializer.Serialize(message)
        );

        // 1. Save to Outbox (Persistence first for reliability)
        await _outboxRepository.CreateAsync(outboxMessage);

        // 2. Try to publish immediately
        try
        {
            var producer = _producerAccessor.GetProducer(producerName);
            if (producer != null)
            {
                await producer.ProduceAsync(message.OrderId, message);

                // 3. Mark as processed immediately if successful
                await _outboxRepository.MarkAsProcessedAsync(outboxMessage.Id, CancellationToken.None);

                _logger.LogInformation("Outbox message {Id} dispatched immediately to producer {Producer}", outboxMessage.Id, producerName);
            }
        }
        catch (Exception ex)
        {
            // Do not throw here. If immediate dispatch fails, the background job will pick it up later.
            _logger.LogWarning(ex, "Failed to dispatch outbox message {Id} immediately. Background job will retry.", outboxMessage.Id);
        }
    }
}
