using System.Security.Claims;
using Application.Features.Forum.Commands;
using Application.Features.Forum.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/forums/comments")]
    public class ForumCommentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID không tìm thấy trong token");

        public ForumCommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostCommentDetail(string id)
        {
            GetPostCommentById query = new()
            {
                Id = id,
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}/replies")]
        public async Task<IActionResult> GetPostCommentReplies(string id, [FromQuery] GetPostCommentReplies query)
        {
            query.ParentId = id;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreatePostCommentCommand command)
        {
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditComment(string id, [FromBody] UpdatePostCommentCommand command)
        {
            command.UserId = currentUserId;
            command.Id = id;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string id)
        {
            DeletePostCommentCommand command = new()
            {
                Id = id,
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        [Authorize]
        public async Task<IActionResult> LikeComment(string id)
        {
            LikePostCommentCommand command = new()
            {
                CommentId = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        [Authorize]
        public async Task<IActionResult> UnlikeComment(string id)
        {
            UnlikePostCommentCommand command = new()
            {
                CommentId = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}