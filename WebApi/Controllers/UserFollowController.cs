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
            
        // Static storage for blocked users (temporary until database implementation)
        private static readonly Dictionary<string, List<object>> _blockedUsersStorage = new();
            
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

        [HttpGet("status/{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> CheckFollowStatus(string targetUserId)
        {
            CheckFollowStatus command = new()
            {
                FollowerId = currentUserId,
                TargetUserId = targetUserId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // Block User APIs
        [HttpPost("blocks/{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> BlockUser(string targetUserId)
        {
            try
            {
                // Initialize storage for current user if not exists
                if (!_blockedUsersStorage.ContainsKey(currentUserId))
                {
                    _blockedUsersStorage[currentUserId] = new List<object>();
                }

                // Check if user is already blocked
                var existingBlock = _blockedUsersStorage[currentUserId]
                    .Cast<dynamic>()
                    .FirstOrDefault(block => block.blockedUserId == targetUserId);

                if (existingBlock != null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "User is already blocked"
                    });
                }

                // Get user information from database
                var getUserQuery = new Application.Features.User.Queries.GetUserById
                {
                    UserId = targetUserId,
                    CurrentUserId = currentUserId
                };

                var userResult = await _mediator.Send(getUserQuery);
                
                Console.WriteLine($"GetUserById result: Success={userResult.Success}, Message={userResult.Message}");
                
                if (!userResult.Success)
                {
                    // If user not found by ID, try to create basic info
                    Console.WriteLine($"User not found by ID: {targetUserId}, creating basic info");
                }

                var userData = userResult.Success ? userResult.Data as dynamic : null;
                Console.WriteLine($"User data: {System.Text.Json.JsonSerializer.Serialize(userData)}");

                // Create new blocked user entry with real user data
                var newBlockedUser = new
                {
                    id = $"block_{DateTime.Now.Ticks}",
                    userId = currentUserId,
                    blockedUserId = targetUserId,
                    blockedUser = new
                    {
                        id = targetUserId,
                        userName = userData?.UserName ?? targetUserId,
                        displayName = userData?.DisplayName ?? $"User {targetUserId}",
                        avatar = userData?.AvatarUrl ?? (string)null
                    },
                    blockedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                // Add to storage
                _blockedUsersStorage[currentUserId].Add(newBlockedUser);

                return Ok(new
                {
                    success = true,
                    message = "User blocked successfully",
                    data = newBlockedUser
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error blocking user: {ex.Message}"
                });
            }
        }

        [HttpDelete("blocks/{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> UnblockUser(string targetUserId)
        {
            try
            {
                if (!_blockedUsersStorage.ContainsKey(currentUserId))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No blocked users found"
                    });
                }

                // Remove user from blocked list
                var blockedUsers = _blockedUsersStorage[currentUserId];
                var userToRemove = blockedUsers
                    .Cast<dynamic>()
                    .FirstOrDefault(block => block.blockedUserId == targetUserId);

                if (userToRemove != null)
                {
                    blockedUsers.Remove(userToRemove);
                    return Ok(new
                    {
                        success = true,
                        message = "User unblocked successfully"
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = "User was not blocked"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error unblocking user: {ex.Message}"
                });
            }
        }

        [HttpGet("blocks")]
        [Authorize]
        public async Task<IActionResult> GetBlockedUsers()
        {
            try
            {
                if (!_blockedUsersStorage.ContainsKey(currentUserId))
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Blocked users retrieved successfully",
                        data = new object[] { }
                    });
                }

                var blockedUsers = _blockedUsersStorage[currentUserId];
                Console.WriteLine($"GetBlockedUsers: Found {blockedUsers.Count} blocked users for user {currentUserId}");
                Console.WriteLine($"Blocked users data: {System.Text.Json.JsonSerializer.Serialize(blockedUsers)}");
                
                return Ok(new
                {
                    success = true,
                    message = "Blocked users retrieved successfully",
                    data = blockedUsers
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error getting blocked users: {ex.Message}"
                });
            }
        }

        [HttpGet("blocks/status/{targetUserId}")]
        [Authorize]
        public async Task<IActionResult> CheckBlockStatus(string targetUserId)
        {
            try
            {
                var isBlocked = false;
                var blockedByMe = false;
                var blockedByUser = false;

                // Check if current user blocked target user
                if (_blockedUsersStorage.ContainsKey(currentUserId))
                {
                    blockedByMe = _blockedUsersStorage[currentUserId]
                        .Cast<dynamic>()
                        .Any(block => block.blockedUserId == targetUserId);
                }

                // Check if target user blocked current user
                if (_blockedUsersStorage.ContainsKey(targetUserId))
                {
                    blockedByUser = _blockedUsersStorage[targetUserId]
                        .Cast<dynamic>()
                        .Any(block => block.blockedUserId == currentUserId);
                }

                isBlocked = blockedByMe || blockedByUser;

                return Ok(new
                {
                    success = true,
                    message = "Block status checked successfully",
                    data = new
                    {
                        isBlocked = isBlocked,
                        blockedByMe = blockedByMe,
                        blockedByUser = blockedByUser
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error checking block status: {ex.Message}"
                });
            }
        }
    }
}