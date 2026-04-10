using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer
{
    public class OrderDeletedConsumer : IMessageHandler<OrderDeletedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderDeletedConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task Handle(IMessageContext context, OrderDeletedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();;

            var idempotencyId = message.IdempotencyId;

            try
            {
                var alreadyProcessed = await inboxRepository.ExistsAsync(idempotencyId, nameof(OrderCreatedConsumer));
                if (alreadyProcessed)
                {
                    return;
                }

                var input = message.Items.Select(x => new OrderItemDTO
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId
                }).ToList();

                var stockResult = await inventoryService.CheckAndDeductInventoryAsync(input);

                if (stockResult.IsSuccess)
                {
                    await inboxRepository.CreateAsync(InboxMessage.Init(idempotencyId, message.OrderId, nameof(OrderDeletedConsumer)));
                }

            }
            catch
            {
                throw; // Để Kafka retry
            }
        }
    }
}