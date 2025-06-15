using Application.Features.Forum.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/comments")]
    public class ForumCommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumCommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}/replies")]

        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreatePostCommentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        // [Authorize]
        public async Task<IActionResult> EditComment(string id, [FromBody] UpdatePostCommentCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        // [Authorize]
        public async Task<IActionResult> DeleteComment(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new DeletePostCommentCommand { Id = id, UserId = "user_002" });
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        // [Authorize]
        public async Task<IActionResult> LikeComment(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new LikePostCommentCommand { CommentId = id, UserId = "user_002" });
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        public async Task<IActionResult> UnlikeComment(string id)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new UnlikePostCommentCommand { CommentId = id, UserId = "user_002" });
            return Ok(result);
        }
    }
}