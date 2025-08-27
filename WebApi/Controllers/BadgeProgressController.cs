using System.Security.Claims;
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
           ?? throw new UnauthorizedAccessException("User ID không tìm thấy trong token");

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

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUserBadgeProgress([FromQuery] string userId)
        {
            var result = await _mediator.Send(new GetBadgeProgress
            {
                UserId = userId
            });

            return Ok(result);
        }
    }
}