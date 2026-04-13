using KafkaFlow;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;
using WebAppExam.InventoryService.Domain.Entity;
using WebAppExam.InventoryService.Application.Repositories;
using Microsoft.Extensions.Logging;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Domain.Exceptions;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedConsumer : IMessageHandler<OrderCreatedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly ICacheLockService _cacheLockService;
    private readonly IMongoClient _mongoClient;
    private readonly IInboxRepository _inboxRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IInventoryService inventoryService,
        IOrderService orderService,
        ICacheLockService cacheLockService,
        IMongoClient mongoClient,
        IInboxRepository inboxRepository,
        ILogger<OrderCreatedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
        _cacheLockService = cacheLockService;
        _mongoClient = mongoClient;
        _inboxRepository = inboxRepository;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, OrderCreatedEvent message)
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


        try
        {
            var alreadyProcessed = await _inboxRepository.ExistsAsync(idempotencyId, nameof(OrderCreatedConsumer));
            if (alreadyProcessed)
            {
                _logger.LogInformation("Message {Id} has been successfully processed before.", idempotencyId);
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
                    await _inboxRepository.CreateAsync(InboxMessage.Init(idempotencyId, message.OrderId, nameof(OrderCreatedConsumer)));
                }

                if(!stockResult.IsSuccess)
                {
                    throw new InsufficientStockException("1", 1, 1);
                }

                await SendMessageReply(message, stockResult.IsSuccess, stockResult.FailReason, input);
            }
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "[Error] - Inventory Handler: OrderId {OrderId}, IdempotencyId {Id}", message.OrderId, idempotencyId);

            var input = message.Items?.Select(x => new OrderItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList() ?? new List<OrderItemDTO>();

            await SendMessageReply(message, false, MessageConstants.InternalServerErrorMessage, input);
            
            // Throw error for KafkaFlow to perform Retry (if configured)
            throw;
        }
        finally
        {
            // 8. RELEASE REDIS LOCK
            await _cacheLockService.ReleaseMultipleLocksAsync(acquiredLocks, lockToken);
        }
    }

    private async Task SendMessageReply(OrderCreatedEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
    {
        var orderReply = OrderReplyDTO.FromResult(
               message.OrderId,
               isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
               reason, MessageConstants.ReplyCreatedType, [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

        await _orderService.SendMessageReply(orderReply, isSuccess, reason);
    }
}