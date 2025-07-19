using System.Security.Claims;
using Application.Features.Follow.Commands;
using Application.Features.Follow.Queries;
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

        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(string userId)
        {
            var result = await _mediator.Send(new GetFollowers { UserId = userId });
            return Ok(result);
        }

        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetFollowing(string userId)
        {
            var result = await _mediator.Send(new GetFollowing { UserId = userId });
            return Ok(result);
        }

        [HttpGet("me/followers")]
        [Authorize]
        public async Task<IActionResult> GetMyFollowers()
        {
            GetFollowers query = new()
            {
                UserId = currentUserId,
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("me/following")]
        [Authorize]
        public async Task<IActionResult> GetMyFollowing()
        {
            GetFollowing query = new()
            {
                UserId = currentUserId,
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

        [HttpDelete("me/followers/{followerId}")]
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