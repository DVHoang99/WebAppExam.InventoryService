using System;
using Grpc.Core;
using WebAppExam.InventoryService.API.Protos;
using WebAppExam.InventoryService.Application.Interfaces;

namespace WebAppExam.InventoryService.API.Services.Grpc;

public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryGrpcService(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }


    public override async Task<GetBatchInventoriesResponse> GetBatchInventories(GetBatchInventoriesRequest request, ServerCallContext context)
    {
        var ids = request.Ids.ToList();
        var result = await _inventoryService.GetBatchInventoryDTOsByIdsAnsyc(ids, context.CancellationToken);

        var response = new GetBatchInventoriesResponse();

        var inventoryDtos = result.Select(x => new InventoryDTO
        {
            ProductId = x.ProductId,
            StockQuantity = x.StockQuantity,
            CorrelationId = x.CorrelationId ?? "", 
            WareHouseId = x.WareHouseId ?? "",
            WareHouse = x.WareHouse != null ? new WareHouseDTO
            {
                Id = x.WareHouse.Id,
                Address = x.WareHouse.Address ?? "",
                OwnerName = x.WareHouse.OwnerName ?? "",
                OwnerEmail = x.WareHouse.OwnerEmail ?? "",
                OwnerPhone = x.WareHouse.OwnerPhone ?? ""
            } : null
        });

        response.Inventories.AddRange(inventoryDtos);

        return response;
    }
}
