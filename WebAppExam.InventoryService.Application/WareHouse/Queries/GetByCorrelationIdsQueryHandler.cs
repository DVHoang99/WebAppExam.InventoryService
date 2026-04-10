using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Inventories.Queries;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.WareHouse.Queries;

public class GetByCorrelationIdsQueryHandler : IRequestHandler<GetByCorrelationIdsQuery, List<GetBatchInventoryDTO>>
{
    private readonly IInventoryRepository _repository;
    private readonly IWareHouseRepository _wareHouseRepository;


    public GetByCorrelationIdsQueryHandler(IInventoryRepository repository, IWareHouseRepository wareHouseRepository)
    {
        _repository = repository;
        _wareHouseRepository = wareHouseRepository;
    }

    public async Task<List<GetBatchInventoryDTO>> Handle(GetByCorrelationIdsQuery request, CancellationToken cancellationToken)
    {
        var inventories = await _repository.GetInventoriesByCorreclationIdsAsync(request.CorrelationIds, cancellationToken);

        if (!inventories.Any())
        {
            return new List<GetBatchInventoryDTO>();
        }

        var wareHouseIds = inventories.Select(i => i.WareHouseId).Distinct().ToList();

        var wareHouses = await _wareHouseRepository.GetByIdsAync(wareHouseIds, cancellationToken);
        var wareHouseMap = wareHouses.ToDictionary(x => x.Id, x => x);

        return inventories.Select(i =>
        {
            var wareHouse = wareHouseMap.GetValueOrDefault(i.WareHouseId);

            var wareHouseDTO = wareHouse != null ? WareHouseDTO.FromResult(wareHouse) : null;

            return GetBatchInventoryDTO.Init(i.ProductId, i.StockQuantity, i.CorrelationId, i.WareHouseId, wareHouseDTO);
        }).ToList();
    }
}