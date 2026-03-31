using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.InventoryService.Application.WareHouse.Commands;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;
using WebAppExam.InventoryService.Application.WareHouse.Queries;

namespace WebAppExam.InventoryService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WareHousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WareHousesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _mediator.Send(new GetWareHouseByIdQuery(id));
            return result != null ? Ok(new { data = result }) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WareHouseDTO wareHouseDTO)
        {
            var command = new CreateWareHouseCommand
            {
                Address = wareHouseDTO.Address,
                OwerName = wareHouseDTO.OwnerName,
                OwerEmail = wareHouseDTO.OwnerEmail,
                OwerPhone = wareHouseDTO.OwnerPhone
            };
            var result = await _mediator.Send(command);
            return Ok(new { data = result });

        }
    }
}
