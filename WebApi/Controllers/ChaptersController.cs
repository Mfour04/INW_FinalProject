using Application.Features.Chapter.Command;
using Application.Features.Chapter.Queries;
using Application.Features.Novel.Commands;
using Application.Features.Novel.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> CreateChapter([FromBody]CreateChapterCommand command)
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
    }
}
