using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.WareHouse.Queries;

public class GetWareHouseByIdQueryhandler : IRequestHandler<GetWareHouseByIdQuery, WareHouseDTO>
{
    private readonly IWareHouseRepository _repository;

    public GetWareHouseByIdQueryhandler(IWareHouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<WareHouseDTO> Handle(GetWareHouseByIdQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return WareHouseDTO.FromResult(res);
    }
}
