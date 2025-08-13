using Application.Features.Novel.Commands;
using Application.Features.NovelFollower.Commands;
using Application.Features.NovelFollower.Queries;
using Application.Features.Tag.Command;
using Application.Features.Tag.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NovelFollowersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NovelFollowersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("created")]
        [Authorize]
        public async Task<IActionResult> CreateNovelFollower([FromBody] CreateNovelFollowerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovelFollower(string id)
        {
            var result = await _mediator.Send(new DeleteNovelFollowerCommand { NovelFollowerId = id });
            return Ok(result);
        }

        [HttpGet("novel/{novelId}")]
        public async Task<IActionResult> GetByNovelId(string novelId)
        {
            var result = await _mediator.Send(new GetFollowerByNovelIdCommand { NovelId = novelId });
            return Ok(result);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetFollowedNovelsByUserId([FromQuery] int page = 0, [FromQuery] int limit = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated."
                });
            }

            var query = new GetFollowerByUserId
            {
                UserId = userId,
                Page = page,
                Limit = limit
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}
