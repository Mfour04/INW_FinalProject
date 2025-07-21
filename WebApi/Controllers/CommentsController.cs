using Application.Features.Comment.Commands;
using Application.Features.Comment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response.Comment;
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
           ?? throw new UnauthorizedAccessException("User ID not found in token");

        public CommentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComments([FromQuery] GetComments query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("novel/{novelId}")]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentsByNovel(string novelId, [FromQuery] GetComments query)
        {
            query.NovelId = novelId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("chapter/{chapterId}")]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentsByChapter(
            string chapterId,
            [FromQuery] string novelId,
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10,
            [FromQuery] bool includeReplies = true)
        {
            try
            {
                var query = new GetComments
                {
                    Page = page,
                    Limit = limit,
                    NovelId = novelId,
                    ChapterId = chapterId,
                    IncludeReplies = includeReplies
                };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command)
        {
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentCommand comment)
        {
            var result = await _mediator.Send(comment);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentByIdAsync(string id)
        {
            var result = await _mediator.Send(new GetCommentById { CommentId = id });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string id)
        {
            DeleteCommentCommand command = new()
            {
                CommentId = id,
                UserId = currentUserId,
                IsAdmin = User.IsInRole("Admin")
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
