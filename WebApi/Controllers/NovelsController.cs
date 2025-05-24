using Application.Features.Novel.Commands;
using Application.Features.Novel.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Shared.Contracts.Respone;

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
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FindCreterias find, [FromQuery] List<SortCreterias> sort)
        {
            var result = await _mediator.Send(new GetNovel
            {
                FindCreterias = find,
                SortCreterias = sort
            });

            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetNovelByIdAsync(string id)
        {
            var result = await _mediator.Send(new GetNovelById { NovelId = id });
            return Ok(result);
        }
        [HttpPost("created")]
        public async Task<IActionResult> CreateNovel([FromBody] CreateNovelCommand command)
        {
            var result = await _mediator.Send(command);
            var createdNovel = result.Data as NovelResponse;
            return CreatedAtAction(nameof(GetNovelById), new { id = createdNovel?.NovelId }, result);
        }
        [HttpPut("updated")]
        public async Task<IActionResult> UpdateNovel([FromBody] UpdateNovelCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovel(string id)
        {
            var result = await _mediator.Send(new DeleteNovelCommand { NovelId = id });
            return Ok(result);
        }
    }
}
