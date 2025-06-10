using Application.Features.Novel.Commands;
using Application.Features.Novel.Queries;
using Domain.Entities.System;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Shared.Contracts.Response;
using System.Collections.Generic;
using System.Globalization;

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
        public async Task<IActionResult> GetAll(
            [FromQuery] string sortBy = "created_at:desc",
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10,
            [FromQuery] string searchTerm = "")
        {
            var query = new GetNovel
            {
                SortBy = sortBy,
                Page = page,
                Limit = limit,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);

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
            return Ok(result);
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
