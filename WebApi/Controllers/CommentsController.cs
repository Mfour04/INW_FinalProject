using Application.Features.Comment.Commands;
using Application.Features.Comment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
           User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new UnauthorizedAccessException("User ID không tìm thấy trong token");

        public CommentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentByIdAsync(string id)
        {
            var result = await _mediator.Send(new GetCommentById { CommentId = id });
            return Ok(result);
        }

        [HttpGet("{id}/reply")]
        public async Task<IActionResult> GetRepliesAsync(string id)
        {
            var result = await _mediator.Send(new GetRepliesComment { CommentId = id });
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command)
        {
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(string id, [FromBody] UpdateCommentCommand command)
        {
            command.CommentId = id;
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string id)
        {
            DeleteCommentCommand command = new()
            {
                CommentId = id
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        [Authorize]
        public async Task<IActionResult> LikeComment(string id, [FromBody] LikeCommentCommand command)
        {
            command.CommentId = id;
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/likes")]
        [Authorize]
        public async Task<IActionResult> UnlikeComment(string id)
        {
            UnlikeCommentCommand command = new()
            {
                CommentId = id,
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
