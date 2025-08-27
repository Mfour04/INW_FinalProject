using Application.Features.ReadingProcess.Command;
using Application.Features.ReadingProcess.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingProcessesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReadingProcessesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetReadingHistory(
            [FromQuery] string userId = null,
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10)
        {
            var query = new GetReadingHistory
            {
                Page = page,
                Limit = limit,
                UserId = userId
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReadingProcess([FromBody] CreateReadingProcessCommand command)
        {
            if (command == null)
            {
                return BadRequest("Invalid reading process data.");
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("update-progress")]
        public async Task<IActionResult> UpdateReadingProgress([FromBody] CreateReadingProcessCommand command)
        {
            if (command == null)
            {
                return BadRequest("Invalid reading process data.");
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReadingProcess(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Reading process ID is required.");
            }

            var result = await _mediator.Send(new DeleteReadingProcessCommand { ReadingProcessId = id });
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
    }
}
