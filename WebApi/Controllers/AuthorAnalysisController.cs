using System.Security.Claims;
using Application.Features.AuthorAnalysis.Earning.Queries;
using Application.Features.AuthorAnalysis.View.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/authors/me")]
    public class AuthorAnalysisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorAnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("earnings/summary")]
        [Authorize]
        public async Task<IActionResult> GetAuthorEarningSummary([FromQuery] GetAuthorEarningsSummary query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("earnings/chart")]
        [Authorize]
        public async Task<IActionResult> GetAuthorEarningChart([FromQuery] GetAuthorEarningsChart query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("earnings/top-novels")]
        [Authorize]
        public async Task<IActionResult> GetAuthorTopEarningNovel([FromQuery] GetAuthorTopNovels query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("views/top-novels")]
        [Authorize]
        public async Task<IActionResult> GetAuthorTopViewedNovel([FromQuery] GetAuthorTopViewedNovels query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("views/top-ratings")]
        [Authorize]
        public async Task<IActionResult> GetAuthorTopRatedNovel([FromQuery] GetAuthorTopRatedNovels query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}