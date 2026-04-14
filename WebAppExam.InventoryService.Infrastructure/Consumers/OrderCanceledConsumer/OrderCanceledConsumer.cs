using KafkaFlow;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;
using WebAppExam.InventoryService.Domain.Entity;
using Microsoft.Extensions.Logging;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Domain.Exceptions;
using StackExchange.Redis;
using Confluent.Kafka;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer;

public class OrderCanceledConsumer : IMessageHandler<OrderCanceledEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly ICacheLockService _cacheLockService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderCanceledConsumer> _logger;

    public OrderCanceledConsumer(
        IInventoryService inventoryService,
        IOrderService orderService,
        ICacheLockService cacheLockService,
        IIdempotencyService idempotencyService,
        IUnitOfWork unitOfWork,
        ILogger<OrderCanceledConsumer> logger)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
        _cacheLockService = cacheLockService;
        _idempotencyService = idempotencyService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, OrderCanceledEvent message)
    {
        var idempotencyId = message.IdempotencyId;
        var lockToken = Guid.NewGuid().ToString();
        var lockKey = $"{CacheKeys.IdempotencyLockPrefix}{idempotencyId}";

        var acquiredLocks = await _cacheLockService.AcquireMultipleLocksAsync(
            new[] { lockKey },
            lockToken,
            TimeSpan.FromSeconds(CommonConstants.LockTimeoutSeconds));

        if (!acquiredLocks.Any())
        {
            _logger.LogWarning("Message {Id} is being processed by another worker.", idempotencyId);
            return;
        }

        using var uow = _unitOfWork;
        await _unitOfWork.StartTransactionAsync();

        try
        {
            var alreadyProcessed = await _idempotencyService.IsProcessedAsync($"{nameof(OrderCanceledConsumer)}:{idempotencyId}");
            if (alreadyProcessed)
            {
                _logger.LogInformation("Message {Id} has been successfully processed before.", idempotencyId);
                await _unitOfWork.RollbackAsync();
                return;
            }

            if (message.Items != null && message.Items.Any())
            {
                var input = message.Items.Select(x => new OrderItemDTO
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId
                }).ToList();

                var stockResult = await _inventoryService.CheckAndDeductInventoryAsync(input);

                if (stockResult.IsSuccess)
                {
                    await _idempotencyService.MarkAsProcessedAsync($"{nameof(OrderCanceledConsumer)}:{idempotencyId}");
                    await SendMessageReply(message, stockResult.IsSuccess, stockResult.FailReason, input);
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    await _idempotencyService.MarkAsProcessedAsync($"{nameof(OrderCanceledConsumer)}:{idempotencyId}");
                    await SendMessageReply(message, false, stockResult.FailReason, input);
                    await _unitOfWork.CommitAsync();
                }
            }
            else
            {
                 await _unitOfWork.RollbackAsync();
            }
        }
        catch (Exception ex) when (ex is RedisException || ex is RedisTimeoutException || ex is RedisConnectionException)
        {
            await RollbackAndThrow(ex, "Redis error during inventory processing", idempotencyId, message.OrderId);
        }
        catch (MongoException ex)
        {
            await RollbackAndThrow(ex, "MongoDB error during inventory processing", idempotencyId, message.OrderId);
        }
        catch (KafkaException ex)
        {
            await RollbackAndThrow(ex, "Kafka error during inventory processing", idempotencyId, message.OrderId);
        }
        catch (Exception ex)
        {
            try { await _unitOfWork.RollbackAsync(); } catch { }
            _logger.LogError(ex, "[Error] - Inventory Handler: OrderId {OrderId}, IdempotencyId {Id}", message.OrderId, idempotencyId);
            throw;
        }
        finally
        {
            await _cacheLockService.ReleaseMultipleLocksAsync(acquiredLocks, lockToken);
        }
    }

    private async Task RollbackAndThrow(Exception ex, string customMessage, string idempotencyId, string orderId)
    {
        try { await _unitOfWork.RollbackAsync(); } catch { }
        _logger.LogError(ex, "[Infrastructure Error] - {Message}: OrderId {OrderId}, IdempotencyId {Id}", customMessage, orderId, idempotencyId);
        throw new DatabaseOperationException(customMessage, ex);
    }

    private async Task SendMessageReply(OrderCanceledEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
    {
        var orderReply = OrderReplyDTO.FromResult(
               message.OrderId,
               isSuccess ? message.Status : OrderStatus.Failed,
               reason, MessageConstants.ReplyCanceledType, [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

        await _orderService.SendMessageReply(orderReply, isSuccess, reason);
    }
}