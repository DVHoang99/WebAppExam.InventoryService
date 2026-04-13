using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Repositories;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, bool>
{
    private readonly IInventoryRepository _repository;

    public UpdateInventoryCommandHandler(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetInventoryByProductIdAndWarehouseIdAsync(
            request.Id,
            request.WareHouseId,
            cancellationToken);

        if (inventory == null)
        {
            throw new Exception("Inventory not found");
        }

        if (inventory.ProcessedUpdateIds.Contains(request.UpdateEventId))
        {
            return true;
        }

        inventory.OverrideStock(request.NewStockQuantity);
        inventory.AddProcessedUpdate(request.UpdateEventId);

        await _repository.UpdateAsync(inventory, cancellationToken);

        return true;
    }
}