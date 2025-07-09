using Application.Features.Comment.Commands;
using Application.Features.Comment.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FindCreterias findCreterias { get; private set; }

        public CommentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10,
            [FromQuery] string novelId = null,
            [FromQuery] string chapterId = null,
            [FromQuery] bool includeReplies = true)
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

        [HttpGet("novel/{novelId}")]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentsByNovel(
            string novelId,
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
                    ChapterId = null,
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

        [HttpPost("created")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated."
                });
            comment.UserId = userId;
            var result = await _mediator.Send(comment);
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
        public async Task<IActionResult> DeleteComment(string id)
        {
            var result = await _mediator.Send(new DeleteCommentCommand { CommentId = id });
            return Ok(result);
        }

        [HttpPost("{id}/likes")]
        public async Task<IActionResult> LikeComment(string id, [FromBody] LikeCommentCommand command)
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            command.CommentId = id;
            command.UserId = "user_002";

            var result = await _mediator.Send(command);
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

            var result = await _mediator.Send(new UnlikeCommentCommand { CommentId = id, UserId = "user_002" });
            return Ok(result);
        }
    }
}
