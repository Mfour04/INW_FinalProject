using Application.Features.Badge.Commands;
using Application.Features.Badge.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/badges")]
    public class BadgeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BadgeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBadges([FromQuery] GetBadges request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBadgeById(string id)
        {
            var result = await _mediator.Send(new GetBadgeById { Id = id });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "System")]
        public async Task<IActionResult> CreateBadge([FromBody] CreateBadgeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "System")]
        public async Task<IActionResult> EditBadge(string id, [FromBody] UpdateBadgeCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "System")]
        public async Task<IActionResult> DeleteBadge(string id)
        {
            var result = await _mediator.Send(new DeleteBadgeCommand { Id = id });
            return Ok(result);
        }

        [HttpGet("enums")]
        public async Task<IActionResult> GetEnums()
        {
            var result = await _mediator.Send(new GetBadgeEnums());
            return Ok(result);
        }
    }
}