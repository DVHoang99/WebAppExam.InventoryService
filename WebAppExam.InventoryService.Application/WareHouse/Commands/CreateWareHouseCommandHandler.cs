using System;
using MediatR;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.Application.WareHouse.Commands;

public class CreateWareHouseCommandHandler : IRequestHandler<CreateWareHouseCommand, WareHouseDTO>
{
    private readonly IWareHouseRepository _repository;

    public CreateWareHouseCommandHandler(IWareHouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<WareHouseDTO> Handle(CreateWareHouseCommand request, CancellationToken cancellationToken)
    {
        var id = Ulid.NewUlid().ToString();
        var wareHouse = new Domain.Entity.WareHouse(id, request.Address, request.OwerName, request.OwerEmail, request.OwerPhone);

        await _repository.AddAsync(wareHouse, cancellationToken);

        return WareHouseDTO.FromResult(wareHouse);
    }
}
