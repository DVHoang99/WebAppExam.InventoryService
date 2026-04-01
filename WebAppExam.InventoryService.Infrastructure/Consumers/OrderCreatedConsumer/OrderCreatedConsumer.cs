using System;
using KafkaFlow;
using KafkaFlow.Producers;
using WebAppExam.InventoryService.Application.Interfaces;

namespace WebAppExam.InventoryService.Infrastructure.Consumers;

public class OrderCreatedConsumer : IMessageHandler<OrderCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly ICacheLockService _lockService;
    private readonly IProducerAccessor _producerAccessor;

    public OrderCreatedConsumer(
        IInventoryRepository inventoryRepo,
        ICacheLockService lockService,
        IProducerAccessor producerAccessor)
    {
        _inventoryRepo = inventoryRepo;
        _lockService = lockService;
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(IMessageContext context, OrderCreatedEvent message)
    {

        Console.WriteLine("===========================================run=========================================================");
        var sortedItems = message.Items
            .OrderBy(x => x.ProductId)
            .ToList();
        var lockKeys = sortedItems
            .Select(x => $"lock:inventory:{x.WareHouseId}:{x.ProductId}")
            .ToList();

        var lockToken = Guid.NewGuid().ToString();
        var acquiredLocks = new List<string>();
        bool isSuccess = true;
        string failReason = string.Empty;

        try
        {
            acquiredLocks = await _lockService.AcquireMultipleLocksAsync(lockKeys, lockToken, TimeSpan.FromSeconds(10));

            if (!acquiredLocks.Any() && lockKeys.Any())
            {
                isSuccess = false;
                failReason = "Hệ thống bận (không thể lấy khóa sản phẩm).";
            }
            if (isSuccess)
            {
                foreach (var item in message.Items)
                {
                    var stock = await _inventoryRepo.GetStockAsync(Ulid.Parse(item.ProductId), Ulid.Parse(item.WareHouseId));
                    if (stock < item.Quantity)
                    {
                        isSuccess = false;
                        failReason = $"Sản phẩm {item.ProductId} không đủ số lượng.";
                        break;
                    }
                }

                if (isSuccess)
                {
                    foreach (var item in message.Items)
                    {
                        await _inventoryRepo.DeductStockAsync(
                            Ulid.Parse(item.ProductId),
                            Ulid.Parse(item.WareHouseId),
                            item.Quantity);
                    }
                }
            }

            var orderReplyProducer = _producerAccessor.GetProducer("order-reply");

            await orderReplyProducer.ProduceAsync(message.OrderId, new
            {
                OrderId = message.OrderId,
                Status = isSuccess ? "Success" : "Failed",
                Reason = failReason
            });
        }
        catch (Exception ex)
        {
            var orderReplyProducer = _producerAccessor.GetProducer("order-reply");
            await orderReplyProducer.ProduceAsync(message.OrderId, new
            {
                OrderId = message.OrderId,
                Status = "Failed",
                Reason = "Lỗi hệ thống nội bộ Inventory"
            });
        }
        finally
        {
            if (acquiredLocks.Any())
            {
                await _lockService.ReleaseMultipleLocksAsync(acquiredLocks, lockToken);
            }
        }
    }
}