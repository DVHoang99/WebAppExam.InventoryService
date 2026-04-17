using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Domain.Enum;
using WebAppExam.InventoryService.Infrastructure.Common;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.CalculateInventoryConsumer
{
    public class CalculateInventoryConsumer : IMessageHandler<OutboxPointer>
    {
        private readonly ILogger<CalculateInventoryConsumer> _logger;
        private readonly OutboxGrpc.OutboxGrpcClient _outboxClient;
        private readonly ICacheLockService _cacheLockService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;


        public CalculateInventoryConsumer(
            ILogger<CalculateInventoryConsumer> logger,
            OutboxGrpc.OutboxGrpcClient outboxClient,
            ICacheLockService cacheLockService,
            IUnitOfWork unitOfWork,
            IIdempotencyService idempotencyService,
            IInventoryService inventoryService,
            IOrderService orderService)
        {
            _logger = logger;
            _outboxClient = outboxClient;
            _cacheLockService = cacheLockService;
            _unitOfWork = unitOfWork;
            _idempotencyService = idempotencyService;
            _inventoryService = inventoryService;
            _orderService = orderService;
        }

        public async Task Handle(IMessageContext context, OutboxPointer pointer)
        {

            var response = await _outboxClient.GetOutboxMessageAsync(new OutboxMessageRequest { Id = pointer.Id });
            var message = JsonSerializer.Deserialize<OutBoxMessageDTO>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message == null)
            {
                _logger.LogError("Failed to deserialize OrderCreatedEvent for Outbox ID: {Id}", pointer.Id);
                return;
            }

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

            await _unitOfWork.StartTransactionAsync();

            try
            {
                var alreadyProcessed = await _idempotencyService.IsProcessedAsync($"{nameof(OrderCreatedConsumer)}:{idempotencyId}");
                if (alreadyProcessed)
                {
                    _logger.LogInformation("Message {Id} has been successfully processed before.", idempotencyId);
                    await _unitOfWork.RollbackAsync();

                    // Feedback: Idempotency hit is treated as success
                    await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 3 });
                    return;
                }

                var item = new OrderItemDTO
                {
                    ProductId = message.ProductId,
                    Quantity = message.Quantity,
                    WareHouseId = message.WareHouseId
                };

                var stockResult = await _inventoryService.CheckAndDeductInventoryAsync(item);
                await SendMessageReply(message.OrderId, stockResult.IsSuccess, stockResult.FailReason, item, pointer.Type);
                await _unitOfWork.CommitAsync();

                await _idempotencyService.MarkAsProcessedAsync($"{nameof(CalculateInventoryConsumer)}:{idempotencyId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing message {Id}", idempotencyId);
                await _unitOfWork.RollbackAsync();
                await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 2 });
            }
        }

        private async Task SendMessageReply(string orderId, bool isSuccess, string reason, OrderItemDTO input, string replyType)
        {
            var orderReply = OrderReplyDTO.FromResult(
                   orderId,
                   isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
                   reason, replyType, OrderDetailDTO.FromResult(input.ProductId, input.Quantity, 0, input.WareHouseId));

            await _orderService.SendMessageReply(orderReply, isSuccess, reason);
        }

        // private async Task RollbackAndThrow(Exception ex, string customMessage, string idempotencyId, string orderId)
        // {
        //     _logger.LogError(ex, customMessage + " OrderId: {OrderId}", orderId);
        //     await _unitOfWork.RollbackAsync();
        //     await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = orderId, Status = 2 });
        //     throw new Exception(customMessage);
        // }
    }

}