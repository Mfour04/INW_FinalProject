using System.Security.Claims;
using Application.Features.Badge.Commands;
using Application.Features.Badge.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/badges/progress")]
    public class BadgeProgressController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
           User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new UnauthorizedAccessException("User ID not found in token");

        public BadgeProgressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyBadgeProgress()
        {
            GetBadgeProgress query = new()
            {
                UserId = currentUserId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{username}/completed")]
        public async Task<IActionResult> GetCompletedBadgeProgress(string username)
        {
            var result = await _mediator.Send(new GetCompletedBadgeProgress { Username = username });
            return Ok(result);
        }

        [HttpPost("init")]
        [Authorize]
        public async Task<IActionResult> InitBadgeForUser()
        {
            InitBadgeProgressCommand command = new()
            {
                UserId = currentUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}