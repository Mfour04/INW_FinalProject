using Application.Features.Badge.Commands;
using Application.Features.Badge.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/badges/progress")]
    public class BadgeProgressController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BadgeProgressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetBadgeProgress()
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new GetBadgeProgress { UserId = "user_002" });
            return Ok(result);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedBadgeProgress()
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;
            var result = await _mediator.Send(new GetCompletedBadgeProgress { UserId = "user_002" });
            return Ok(result);
        }


        // [HttpGet("/{userId}/completed")]
        [HttpPost("init")]
        public async Task<IActionResult> InitBadgeForUser()
        {
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });
            // command.UserId = userId;

            var result = await _mediator.Send(new InitBadgeProgressCommand { UserId = "user_002" });
            return Ok(result);
        }
    }
}