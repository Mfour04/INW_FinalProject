using Application.Features.Chapter.Queries;
using Application.Features.Novel.Commands;
using Application.Features.Novel.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using Shared.Helpers;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FindCreterias FindCreterias { get; private set; }
        public SortCreterias SortCreterias { get; private set; }

        public NovelsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] string sortBy = "created_at:desc",
        //    [FromQuery] int page = 0,
        //    [FromQuery] int limit = 10,
        //    [FromQuery] string searchTerm = "",
        //    [FromQuery] string searchTagTerm = null)
        //{
        //    var tagNames = SystemHelper.ParseTagNames(searchTagTerm);

        //    var query = new GetNovel
        //    {
        //        SortBy = sortBy,
        //        Page = page,
        //        Limit = limit,
        //        SearchTerm = searchTerm,
        //        SearchTagTerm = tagNames
        //    };

        //    var result = await _mediator.Send(query);

        //    return Ok(result);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetNovel query)
        {
            // Nếu tag truyền dạng chuỗi "Fantasy,Romance", ta tách ra
            if (query.SearchTagTerm?.Count == 1 && query.SearchTagTerm[0].Contains(','))
            {
                query.SearchTagTerm = SystemHelper.ParseTagNames(query.SearchTagTerm[0]);
            }

            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetNovelByIdAsync(string id, [FromQuery] int page = 0, [FromQuery] int limit = 10
            , [FromQuery] string sortBy = "chapter_number:asc", [FromQuery] int? chapterNumber = null)
        {
            string? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            var result = await _mediator.Send(new GetNovelById
            {
                NovelId = id,
                UserId = userId,
                Page = page,
                Limit = limit,
                SortBy = sortBy,
                ChapterNumber = chapterNumber
            });

            return Ok(result);
        }

        [HttpGet("get-by-authorid")]
        [Authorize]
        public async Task<IActionResult> GetByAuthorId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated."
                });
            var result = await _mediator.Send(new GetNovelByAuthorId
            {
                AuthorId = userId
            });
            return Ok(result);
        }

        [HttpPost("created")]
        [Authorize]
        public async Task<IActionResult> CreateNovel([FromForm] CreateNovelCommand command)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated."
                });
            command.AuthorId = userId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("updated")]
        [Authorize]
        public async Task<IActionResult> UpdateNovel([FromForm] UpdateNovelCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNovel(string id)
        {
            var result = await _mediator.Send(new DeleteNovelCommand
            {
                NovelId = id
            });

            return Ok(result);
        }

        [HttpPost("{id}/buy")]
        public async Task<IActionResult> BuyNovel(string id, [FromBody] BuyNovelCommand command)
        {
            command.NovelId = id;
            command.UserId = "user_002";

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("hide/{novelId}")]
        [Authorize]
        public async Task<IActionResult> HideNovel(string novelId)
        {
            var result = await _mediator.Send(new HideNovelCommand
            {
                NovelId = novelId
            });

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
