
using Grpc.Core;
using MediatR;
using WebAppExam.GrpcContracts.Protos;
using WebAppExam.InventoryService.Application.WareHouse.Queries;

namespace WebAppExam.InventoryService.API.Services.Grpc;

public class WarehouseGrpcService : WarehouseGrpc.WarehouseGrpcBase
{
    private readonly IMediator _mediator;

    public WarehouseGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetWarehouseResponse> GetWarehouse(GetWarehouseRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(GetWareHouseByIdQuery.Init(request.Id));

        return new GetWarehouseResponse
        {
            Id = result.Id,
            Address = result.Address,
            OwnerName = result.OwnerName,
            OwnerEmail = result.OwnerEmail,
            OwnerPhone = result.OwnerPhone
        };
    }
}
