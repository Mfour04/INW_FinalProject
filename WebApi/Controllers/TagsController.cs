using Application.Features.Novel.Commands;
using Application.Features.Tag.Command;
using Application.Features.Tag.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TagsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTag()
        {
            var result = await _mediator.Send(new GetTag());
            return Ok(result);
        }

        [HttpPost("created")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("updated")]
        public async Task<IActionResult> UpdateTag([FromBody] UpdateTagCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(string id)
        {
            var result = await _mediator.Send(new DeleteTagCommand { TagId = id });
            return Ok(result);
        }
    }
}
