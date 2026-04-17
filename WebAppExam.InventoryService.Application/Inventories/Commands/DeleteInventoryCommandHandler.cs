using MediatR;
using WebAppExam.InventoryService.Application.Repositories;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class DeleteInventoryCommandHandler : IRequestHandler<DeleteInventoryCommand, bool>
{
    private readonly IInventoryRepository _repository;

    public DeleteInventoryCommandHandler(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetInventoryByProductIdAndWarehouseIdAsync(request.Id, request.WareHouseId, cancellationToken);

        if (inventory == null)
        {
            return false;
        }

        await _repository.DeleteAsync(inventory.Id, cancellationToken);

        return true;
    }
}