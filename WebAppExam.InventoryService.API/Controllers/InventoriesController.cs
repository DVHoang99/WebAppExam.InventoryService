using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.InventoryService.Application.Inventories.Commands;
using WebAppExam.InventoryService.Application.Inventories.DTOs;
using WebAppExam.InventoryService.Application.Inventories.Queries;
using WebAppExam.InventoryService.Application.WareHouse.DTOs;

namespace WebAppExam.InventoryService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryDTO input)
        {
            var command = new CreateInventoryCommand(input);
            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetByCorrelationIds([FromBody] GetBatchInventoryRequest request)
        {
            var query = new GetByCorrelationIdsQuery(request);
            var inventories = await _mediator.Send(query);
            return Ok(new { data = inventories });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _mediator.Send(new GetInventoryByIdQuery(id));
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] InventoryRequestDTO request)
        {
            var command = new UpdateInventoryCommand(id, request.WareHouseId, request.Stock, request.UpdateEventId);

            var result = await _mediator.Send(command);

            return Ok(new { data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string wareHouseId)
        {
            var command = new DeleteInventoryCommand(id, wareHouseId);
            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }
    }
}
