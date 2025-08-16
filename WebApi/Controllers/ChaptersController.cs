using Application.Features.Chapter.Commands;
using Application.Features.Chapter.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChaptersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FindCreterias FindCreterias { get; private set; }
        private string currentUserId =>
           User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new UnauthorizedAccessException("User ID not found in token");

        public ChaptersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
           [FromQuery] int page = 0,
           [FromQuery] int limit = 10)
        {
            var query = new GetChapter
            {
                Page = page,
                Limit = limit
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost("created")]
        [Authorize]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChapterByIdAsync(string id)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            GetChapterById query = new()
            {
                ChapterId = id,
                IpAddress = ipAddress
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("updated")]
        [Authorize]
        public async Task<IActionResult> UpdateChapter([FromBody] UpdateChapterCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteChapter(string id)
        {
            var result = await _mediator.Send(new DeleteChapterCommand { ChapterId = id });
            return Ok(result);
        }

        [HttpPost("{id}/buy")]
        [Authorize]
        public async Task<IActionResult> BuyChapter(string id, [FromBody] BuyChapterCommand command)
        {
            command.ChapterId = id;
            command.UserId = currentUserId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("get-chapter-by-novelId")]
        public async Task<IActionResult> GetAllChapterByNovelId(string novelId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new GetAllChapterByNovelId { NovelId = novelId, UserId = userId });

            return Ok(result);
        }

        [HttpPut("update-hide-chapter/{chapterId}")]
        [Authorize]
        public async Task<IActionResult> HidevsUnhideChapter(string chapterId, [FromQuery] bool isPublic)
        {
            var result = await _mediator.Send(new UpdateHideChapterStatusCommand
            {
                ChapterId = chapterId,
                IsPublic = isPublic
            });

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update-lock-chapters")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLockChapters([FromBody] UpdateLockChapterStatusCommand request)
        {
            if (request == null || request.ChapterIds == null || !request.ChapterIds.Any())
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Vui lòng truyền danh sách chapterIds."
                });
            }

            var result = await _mediator.Send(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}/comments")]
        public async Task<ActionResult> GetComments(string id, [FromQuery] GetChapterComments query)
        {
            query.ChapterId = id;

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
