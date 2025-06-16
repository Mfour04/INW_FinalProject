using System.Security.Claims;
using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/posts")]
    public class ForumPostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumPostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        // [Authorize]
        public async Task<IActionResult> GetPosts([FromQuery] GetPosts request)
        {
            // request.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        // [Authorize]
        public async Task<IActionResult> GetHotelById(string id)
        {
            var result = await _mediator.Send(new GetPostById { Id = id });
            return Ok(result);
        }

        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        // [Authorize]
        public async Task<IActionResult> EditPost(string id, [FromBody] UpdatePostCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        // [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new DeletePostCommand { Id = id, UserId = "user_002" });
            return Ok(result);
        }

        [HttpGet("{id}/comments")]
        // [Authorize]
        public async Task<IActionResult> GetPostComments(string id, [FromQuery] GetPostComments request)
        {
            GetPostComments query = new()
            {
                PostId = id,
                Page = request.Page,
                Limit = request.Limit,
                SortBy = request.SortBy
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        // [Authorize]
        public async Task<IActionResult> LikePost(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new LikePostCommand { PostId = id, UserId = "user_002" });
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        public async Task<IActionResult> UnlikePost(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new UnlikePostCommand { PostId = id, UserId = "user_002" });
            return Ok(result);
        }

    }
}