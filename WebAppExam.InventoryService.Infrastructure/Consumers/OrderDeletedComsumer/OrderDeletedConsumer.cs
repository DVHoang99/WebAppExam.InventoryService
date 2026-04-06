using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

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

            var input = message.Items.Select(x => new OrderItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList();

            await inventoryService.CheckAndDeductInventoryAsync(input);
        }
    }
}