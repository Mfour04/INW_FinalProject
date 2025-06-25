using Application.Features.Chapter.Command;
using Application.Features.Chapter.Commands;
using Application.Features.Chapter.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChaptersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FindCreterias FindCreterias { get; private set; }

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
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetChapterByIdAsync(string id)
        {
            var result = await _mediator.Send(new GetChapterById { ChapterId = id });
            return Ok(result);
        }

        [HttpPut("updated")]
        public async Task<IActionResult> UpdateNovel([FromBody] UpdateChapterCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovel(string id)
        {
            var result = await _mediator.Send(new DeleteChapterCommand { ChapterId = id });
            return Ok(result);
        }
        [HttpPost("release-chapter")]
        public async Task<IActionResult> ReleaseChapter()
        {
            var result = await _mediator.Send(new ScheduleChapterReleaseCommand());
            return Ok(result);
        }
        
        [HttpPost("{id}/buy")]
        public async Task<IActionResult> BuyNovel(string id, [FromBody] BuyChapterCommand command)
        {
            command.ChapterId = id;
            command.UserId = "user_002";

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
