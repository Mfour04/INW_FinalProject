using Application.Features.Novel.Commands;
using Application.Features.NovelFollower.Commands;
using Application.Features.NovelFollower.Queries;
using Application.Features.Tag.Command;
using Application.Features.Tag.Queries;
using MediatR;
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
        public async Task<IActionResult> CreateNovelFollower([FromBody] CreateNovelFollowerCommand command)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "You should be login first."
                });
            command.UserId = userId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovelFollower(string id)
        {
            var result = await _mediator.Send(new DeleteNovelFollowerCommand { NovelFollowerId = id });
            return Ok(result);
        }

        [HttpGet("{novelId}")]
        public async Task<IActionResult> GetByNovelId(string novelId)
        {
            var result = await _mediator.Send(new GetFollowerByNovelIdCommand { NovelId = novelId });
            return Ok(result);
        }
    }
}
