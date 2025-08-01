using System.Security.Claims;
using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/posts")]
    public class ForumPostController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        public ForumPostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] GetPosts query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(string id)
        {
            GetPostById query = new()
            {
                Id = id,
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostCommand command)
        {
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditPost(string id, [FromBody] UpdatePostCommand command)
        {
            command.UserId = currentUserId;
            command.Id = id;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            DeletePostCommand command = new()
            {
                Id = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetPostComments(string id, [FromQuery] GetPostComments query)
        {
            GetPostComments newQuery = new()
            {
                PostId = id,
                Page = query.Page,
                Limit = query.Limit,
                SortBy = query.SortBy
            };

            var result = await _mediator.Send(newQuery);
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        [Authorize]
        public async Task<IActionResult> LikePost(string id)
        {
            LikePostCommand command = new()
            {
                PostId = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        public async Task<IActionResult> UnlikePost(string id)
        {
            UnlikePostCommand command = new()
            {
                PostId = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

    }
}