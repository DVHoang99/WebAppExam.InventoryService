using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.InventoryService.Application.Interfaces;

namespace WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer
{
    public class OrderDeletedConsumer : IMessageHandler<OrderDeletedEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProducerAccessor _producerAccessor;

        public OrderDeletedConsumer(IServiceProvider serviceProvider, IProducerAccessor producerAccessor)
        {
            _serviceProvider = serviceProvider;
            _producerAccessor = producerAccessor;
        }
        public async Task Handle(IMessageContext context, OrderDeletedEvent message)
        {
            bool isSuccess = true;

            using var scope = _serviceProvider.CreateScope();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

            foreach (var item in message.Items)
            {
                var stock = await inventoryRepo.GetStockAsync(Ulid.Parse(item.ProductId), Ulid.Parse(item.WareHouseId));
                if (stock < item.Quantity)
                {
                    isSuccess = false;
                    break;
                }
            }

            if (isSuccess)
            {
                foreach (var item in message.Items)
                {
                    await inventoryRepo.DeductStockAsync(
                        Ulid.Parse(item.ProductId),
                        Ulid.Parse(item.WareHouseId),
                        item.Quantity);
                }
            }
        }
    }
}