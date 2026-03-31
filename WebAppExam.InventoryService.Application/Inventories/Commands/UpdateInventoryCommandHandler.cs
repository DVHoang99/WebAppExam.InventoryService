using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;

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
        // 1. Get the existing entity
        var inventory = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (inventory == null)
        {
            throw new Exception("Inventory not found");
        }

        // 2. Execute domain logic to update state
        inventory.UpdateStock(request.NewStockQuantity);

        // 3. Save the updated entity
        await _repository.UpdateAsync(inventory, cancellationToken);

        return true;
    }
}