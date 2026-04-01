using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Application.Inventories.Commands;

public class CreateInventoryCommandhandler : IRequestHandler<CreateInventoryCommand, InventoryDTO>
{
    private readonly IInventoryRepository _repository;

    public CreateInventoryCommandhandler(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryDTO> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var existingInventory = await _repository.GetInventoryByCorreclationIdAsync(request.CorrelationId, request.ProductId, request.WareHouseId, cancellationToken);

        if (existingInventory != null)
        {
            return InventoryDTO.FromResult(existingInventory);
        }

        var id = Ulid.NewUlid().ToString();

        var inventory = new Inventory(id, request.ProductId, request.StockQuantity, request.CorrelationId, request.WareHouseId);

        await _repository.AddAsync(inventory, cancellationToken);

        return InventoryDTO.FromResult(inventory);
    }
}
