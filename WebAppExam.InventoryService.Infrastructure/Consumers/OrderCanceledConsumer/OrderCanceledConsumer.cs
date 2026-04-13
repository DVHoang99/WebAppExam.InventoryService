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

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer;

public class OrderCanceledConsumer : IMessageHandler<OrderCanceledEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IOrderService _orderService;
    private readonly ICacheLockService _cacheLockService;
    private readonly IMongoClient _mongoClient;
    private readonly IInboxRepository _inboxRepository;
    private readonly ILogger<OrderCanceledConsumer> _logger;

    public OrderCanceledConsumer(
        IInventoryService inventoryService,
        IOrderService orderService,
        ICacheLockService cacheLockService,
        IMongoClient mongoClient,
        IInboxRepository inboxRepository,
        ILogger<OrderCanceledConsumer> logger)
    {
        _inventoryService = inventoryService;
        _orderService = orderService;
        _cacheLockService = cacheLockService;
        _mongoClient = mongoClient;
        _inboxRepository = inboxRepository;
        _logger = logger;
    }

    public async Task Handle(IMessageContext context, OrderCanceledEvent message)
    {
        var idempotencyId = message.IdempotencyId;
        var lockToken = Guid.NewGuid().ToString();
        var lockKey = $"lock:idempotency:{idempotencyId}";

        var acquiredLocks = await _cacheLockService.AcquireMultipleLocksAsync(
            new[] { lockKey },
            lockToken,
            TimeSpan.FromSeconds(30));

        if (!acquiredLocks.Any())
        {
            _logger.LogWarning("Message {Id} đang được xử lý bởi worker khác.", idempotencyId);
            return;
        }

        try
        {
            var alreadyProcessed = await _inboxRepository.ExistsAsync(idempotencyId, nameof(OrderCanceledConsumer));
            if (alreadyProcessed)
            {
                _logger.LogInformation("Message {Id} đã được xử lý thành công trước đó.", idempotencyId);
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
                    await _inboxRepository.CreateAsync(InboxMessage.Init(idempotencyId, message.OrderId, nameof(OrderCanceledConsumer)));
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

            await SendMessageReply(message, false, "Lỗi hệ thống nội bộ Inventory", input);
            
            throw;
        }
        finally
        {
            await _cacheLockService.ReleaseMultipleLocksAsync(acquiredLocks, lockToken);
        }
    }

    private async Task SendMessageReply(OrderCanceledEvent message, bool isSuccess, string reason, List<OrderItemDTO> input)
    {
        var orderReply = OrderReplyDTO.FromResult(
               message.OrderId,
               isSuccess ? message.Status : OrderStatus.Failed,
               reason, "canceled", [.. input.Select(x => OrderDetailDTO.FromResult(x.ProductId, x.Quantity, 0, x.WareHouseId))]);

        await _orderService.SendMessageReply(orderReply, isSuccess, reason);
    }
}