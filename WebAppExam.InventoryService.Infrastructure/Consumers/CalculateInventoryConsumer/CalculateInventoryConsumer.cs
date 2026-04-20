using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Orders.Services;
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

        public CalculateInventoryConsumer(
            ILogger<CalculateInventoryConsumer> logger,
            OutboxGrpc.OutboxGrpcClient outboxClient,
            ICacheLockService cacheLockService,
            IUnitOfWork unitOfWork,
            IIdempotencyService idempotencyService,
            IInventoryService inventoryService)
        {
            _logger = logger;
            _outboxClient = outboxClient;
            _cacheLockService = cacheLockService;
            _unitOfWork = unitOfWork;
            _idempotencyService = idempotencyService;
            _inventoryService = inventoryService;
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
           [lockKey],
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
                    // status 3: Processed successfully (idempotency hit)
                    await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 3 });
                    return;
                }

                var item = new OrderItemDTO
                {
                    ProductId = message.ProductId,
                    Quantity = message.Quantity,
                    WareHouseId = message.WareHouseId
                };

                var (isSuccess, failReason) = await _inventoryService.CheckAndDeductInventoryAsync(item);
                //await SendMessageReply(message.OrderId, isSuccess, failReason, item, pointer.Type);
                if (!isSuccess)
                {
                    // Feedback: Failure
                    // status 2: Failed to process
                    await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 2 });
                    _logger.LogError($"Inventory check failed for OrderId: {message.OrderId}, ProductId: {message.ProductId}. Reason: {failReason}");
                    await _unitOfWork.RollbackAsync();
                }

                await _idempotencyService.MarkAsProcessedAsync($"{nameof(CalculateInventoryConsumer)}:{idempotencyId}");
                await _unitOfWork.CommitAsync();
                return;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing message {Id}", idempotencyId);

                await _unitOfWork.RollbackAsync();

                // Feedback: Failure
                // status 2: Failed to process
                await _outboxClient.UpdateOutboxStatusAsync(new UpdateStatusRequest { Id = pointer.Id, Status = 2 });
            }
        }

        // private async Task SendMessageReply(string orderId, bool isSuccess, string reason, OrderItemDTO input, string replyType)
        // {
        //     var orderReply = OrderReplyDTO.FromResult(
        //            orderId,
        //            isSuccess ? OrderStatus.Pending : OrderStatus.Failed,
        //            reason, replyType, OrderDetailDTO.FromResult(input.ProductId, input.Quantity, 0, input.WareHouseId));

        //     await _orderService.SendMessageReply(orderReply, isSuccess, reason);
        // }
    }

}