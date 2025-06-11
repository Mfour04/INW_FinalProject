using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums")]
    public class ForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("posts")]
        // [Authorize]
        public async Task<IActionResult> GetPosts([FromQuery] GetPosts query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("posts")]
        // [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}