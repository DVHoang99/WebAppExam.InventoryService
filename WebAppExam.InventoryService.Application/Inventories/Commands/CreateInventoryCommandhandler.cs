using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class CreateInventoryCommandhandler : IRequestHandler<CreateInventoryCommand, string>
{
    private readonly IInventoryRepository _repository;

    public CreateInventoryCommandhandler(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = new Inventory(request.Id, request.ProductId, request.StockQuantity, request.Name, request.Address);

        await _repository.AddAsync(inventory, cancellationToken);

        return inventory.Id;
    }
}
