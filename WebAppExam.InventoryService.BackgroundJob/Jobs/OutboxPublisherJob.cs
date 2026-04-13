using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Microsoft.Extensions.Logging;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.BackgroundJob.Jobs;

public class OutboxPublisherJob : IOutboxPublisherJob
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<OutboxPublisherJob> _logger;

    public OutboxPublisherJob(
        IOutboxRepository outboxRepository,
        IProducerAccessor producerAccessor,
        ILogger<OutboxPublisherJob> logger)
    {
        _outboxRepository = outboxRepository;
        _producerAccessor = producerAccessor;
        _logger = logger;
    }

    public async Task ProcessOutboxMessagesAsync()
    {
        try
        {
            var messages = await _outboxRepository.GetUnprocessedMessagesAsync(50, CancellationToken.None);

            foreach (var message in messages)
            {
                try
                {
                    if (message.Topic == KafkaProducers.OrderReplyProducer ||
                        message.Topic == KafkaProducers.OrderUpdatedReplyProducer ||
                        message.Topic == KafkaProducers.OrderCanceledReplyProducer)
                    {
                        var orderReply = JsonSerializer.Deserialize<OrderReplyDTO>(message.Payload);
                        if (orderReply != null)
                        {
                            var producer = _producerAccessor.GetProducer(message.Topic);
                            await producer.ProduceAsync(orderReply.OrderId, orderReply);
                        }
                    }

                    await _outboxRepository.MarkAsProcessedAsync(message.Id, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {Id} via Hangfire", message.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox messages via Hangfire");
            throw; // Rethrow for Hangfire retry mechanism
        }
    }
}
