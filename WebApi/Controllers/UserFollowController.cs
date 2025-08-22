using System.Security.Claims;
using Application.Features.UserFollow.Commands;
using Application.Features.UserFollow.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/follows")]
    public class UserFollowController : ControllerBase
    {
        private readonly IMediator _mediator;
        private string currentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
            
		public UserFollowController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{username}/followers")]
        public async Task<IActionResult> GetFollowers(string username)
        {
            var result = await _mediator.Send(new GetFollowers { Username = username });
            return Ok(result);
        }

        [HttpGet("{username}/following")]
        public async Task<IActionResult> GetFollowing(string username)
        {
            var result = await _mediator.Send(new GetFollowing { Username = username });
            return Ok(result);
        }

        [HttpGet("status/{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> CheckFollowStatus(string targetUserId)
        {
            var query = new Application.Features.UserFollow.Queries.CheckFollowStatus
            {
                FollowerId = currentUserId,
                TargetUserId = targetUserId
            };
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> Follow(string targetUserId)
        {
            FollowUserCommand command = new()
            {
                FollowerId = currentUserId,
                FollowingId = targetUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> Unfollow(string targetUserId)
        {
            UnfollowUserCommand command = new()
            {
                FollowerId = currentUserId,
                FollowingId = targetUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("followers/{followerId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFollower(string followerId)
        {
            RemoveFollowerCommand command = new()
            {
                CurrentUserId = currentUserId,
                FollowerToRemoveId = followerId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}