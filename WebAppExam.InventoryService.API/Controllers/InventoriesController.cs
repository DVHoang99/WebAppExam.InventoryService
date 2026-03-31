using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.InventoryService.Application.Inventories.Commands;
using WebAppExam.InventoryService.Application.Inventories.Queries;

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
        public async Task<IActionResult> Create(CreateInventoryCommand command)
        {
            // Send command to MediatR
            var id = await _mediator.Send(command);
            return Ok(new { Id = id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            // Send query to MediatR
            var result = await _mediator.Send(new GetInventoryByIdQuery(id));
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateInventoryCommand command)
        {
            // Ensure the ID in the URL matches the ID in the request body
            if (id != command.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            // Send command to MediatR to handle update
            var result = await _mediator.Send(command);

            // Return 404 if inventory doesn't exist, otherwise return 204 No Content
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: DELETE api/inventories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Prepare and send the delete command
            var command = new DeleteInventoryCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
