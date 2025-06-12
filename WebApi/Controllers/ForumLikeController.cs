using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/likes")]
    public class ForumLikeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumLikeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("posts/{postId}")]
        // [Authorize]
        public async Task<IActionResult> LikePost(string postId)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new LikePostCommand { PostId = postId, UserId = "user_002" });
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        public async Task<IActionResult> UnlikePost(string postId)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new UnlikePostCommand { PostId = postId, UserId = "user_002" });
            return Ok(result);
        }

    }
}