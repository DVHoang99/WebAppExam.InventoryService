using System;
using Grpc.Core;
using MediatR;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.InventoryService.Application.Inventories.Commands;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Inventories.Queries;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.API.Services.Grpc;

public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IInventoryService _inventoryService;

    public InventoryGrpcService(IMediator mediator, IInventoryService inventoryService)
    {
        _mediator = mediator;
        _inventoryService = inventoryService;
    }

    public override async Task<CreateInventoryResponse> CreateInventory(CreateInventoryRequest request, ServerCallContext context)
    {
        var inventoryDto = Application.Inventories.DTOs.InventoryDTO.FromResult(
            request.Inventory.ProductId,
            request.Inventory.StockQuantity,
            request.Inventory.CorrelationId,
            request.Inventory.WareHouseId
        );

        var command = new CreateInventoryCommand(inventoryDto);
        var result = await _mediator.Send(command, context.CancellationToken);

        var response = new CreateInventoryResponse
        {
            Inventory = MapToProtoInventory(result)
        };

        return response;
    }

    public override async Task<GetInventoryResponse> GetInventory(GetInventoryRequest request, ServerCallContext context)
    {
        var query = new GetInventoryByIdQuery(request.Id);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Inventory with id {request.Id} not found"));
        }

        var response = new GetInventoryResponse
        {
            Inventory = MapToProtoInventory(result)
        };

        return response;
    }

    public override async Task<UpdateInventoryResponse> UpdateInventory(UpdateInventoryRequest request, ServerCallContext context)
    {
        var command = new UpdateInventoryCommand(request.Id, request.WareHouseId, request.Stock, request.UpdateEventId);
        var result = await _mediator.Send(command, context.CancellationToken);

        // Since UpdateInventoryCommand returns bool, we need to get the updated inventory
        if (result)
        {
            var query = new GetInventoryByIdQuery(request.Id);
            var updatedInventory = await _mediator.Send(query, context.CancellationToken);

            var response = new UpdateInventoryResponse
            {
                Inventory = MapToProtoInventory(updatedInventory)
            };

            return response;
        }
        else
        {
            throw new RpcException(new Status(StatusCode.Internal, "Failed to update inventory"));
        }
    }

    public override async Task<DeleteInventoryResponse> DeleteInventory(DeleteInventoryRequest request, ServerCallContext context)
    {
        var command = new DeleteInventoryCommand(request.Id, request.WareHouseId);
        var result = await _mediator.Send(command, context.CancellationToken);

        var response = new DeleteInventoryResponse
        {
            Success = result,
            Message = result ? "Inventory deleted successfully" : "Failed to delete inventory"
        };

        return response;
    }

    public override async Task<GetBatchInventoriesResponse> GetBatchInventories(GetBatchInventoriesRequest request, ServerCallContext context)
    {
        var ids = request.Ids.ToList();
        var result = await _inventoryService.GetBatchInventoryDTOsByIdsAnsyc(ids, context.CancellationToken);

        var response = new GetBatchInventoriesResponse();

        var inventoryDtos = result.Select(x => new WebAppExam.GrpcContracts.Protos.InventoryDTO
        {
            ProductId = x.ProductId,
            StockQuantity = x.StockQuantity,
            CorrelationId = x.CorrelationId ?? "",
            WareHouseId = x.WareHouseId ?? "",
            WareHouse = x.WareHouse != null ? new WebAppExam.GrpcContracts.Protos.WareHouseDTO
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

    public override async Task<GetByCorrelationIdsResponse> GetByCorrelationIds(GetByCorrelationIdsRequest request, ServerCallContext context)
    {
        var correlationIds = request.CorrelationIds.ToList();
        var query = new GetByCorrelationIdsQuery(new GetBatchInventoryRequest { CorrelationIds = correlationIds });
        var result = await _mediator.Send(query, context.CancellationToken);

        var response = new GetByCorrelationIdsResponse();

        var inventoryDtos = result.Select(x => new WebAppExam.GrpcContracts.Protos.InventoryDTO
        {
            ProductId = x.ProductId,
            StockQuantity = x.StockQuantity,
            CorrelationId = x.CorrelationId ?? "",
            WareHouseId = x.WareHouseId ?? "",
            WareHouse = x.WareHouse != null ? new WebAppExam.GrpcContracts.Protos.WareHouseDTO
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

    private static WebAppExam.GrpcContracts.Protos.InventoryDTO MapToProtoInventory(WebAppExam.InventoryService.Application.Inventories.DTOs.InventoryDTO inventory)
    {
        return new WebAppExam.GrpcContracts.Protos.InventoryDTO
        {
            ProductId = inventory.ProductId,
            StockQuantity = inventory.StockQuantity,
            CorrelationId = inventory.CorrelationId ?? "",
            WareHouseId = inventory.WareHouseId ?? "",
            WareHouse = null // Regular InventoryDTO doesn't include warehouse details
        };
    }

    private static WebAppExam.GrpcContracts.Protos.InventoryDTO MapToProtoInventory(GetBatchInventoryDTO inventory)
    {
        return new WebAppExam.GrpcContracts.Protos.InventoryDTO
        {
            ProductId = inventory.ProductId,
            StockQuantity = inventory.StockQuantity,
            CorrelationId = inventory.CorrelationId ?? "",
            WareHouseId = inventory.WareHouseId ?? "",
            WareHouse = inventory.WareHouse != null ? new WebAppExam.GrpcContracts.Protos.WareHouseDTO
            {
                Id = inventory.WareHouse.Id,
                Address = inventory.WareHouse.Address ?? "",
                OwnerName = inventory.WareHouse.OwnerName ?? "",
                OwnerEmail = inventory.WareHouse.OwnerEmail ?? "",
                OwnerPhone = inventory.WareHouse.OwnerPhone ?? ""
            } : null
        };
    }
}
