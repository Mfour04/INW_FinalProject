using System.Security.Claims;
using Application.Features.AuthorEarning.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/earnings")]
    public class AuthorEarningController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        public AuthorEarningController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetAuthorEarningSummary([FromQuery] GetAuthorEarningsSummary query)
        {
            query.UserId = currentUserId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("chart")]
        [Authorize]
        public async Task<IActionResult> GetAuthorEarningChart([FromQuery] GetAuthorEarningsChart query)
        {
            query.UserId = currentUserId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("top-novels")]
        [Authorize]
        public async Task<IActionResult> GetAuthorTopEarningNovel([FromQuery] GetAuthorTopEarningNovels query)
        {
            query.UserId = currentUserId;

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}