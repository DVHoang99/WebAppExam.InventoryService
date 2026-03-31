using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.Queries;

public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, InventoryDTO>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryByIdQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryDTO> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var res = await _inventoryRepository.GetByIdAsync(request.Id, cancellationToken);

        return new InventoryDTO
        {
            Id = res.Id,
            ProductId = res.ProductId,
            StockQuantity = res.StockQuantity,
        };
    }
}
