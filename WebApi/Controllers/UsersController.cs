using Application.Auth.Commands;
using Application.Features.Novel.Queries;
using Application.Features.User.Feature;
using Application.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly JwtHelpers _jwtHelpers;
        
        public UsersController(IMediator mediator, JwtHelpers jwtHelpers)
        {
            _mediator = mediator;
            _jwtHelpers = jwtHelpers;
        }

        // Đăng ký người dùng mới
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand request)
        {
            var result = await _mediator.Send(request);

            if (!result.Success)
                return BadRequest(result);

            return Created("register", result);
        }

        // Đăng nhập và lấy JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand request)
        {
            var result = await _mediator.Send(request);

            if (!result.Success)
                return BadRequest(result);

            // Gắn JWT vào cookie nếu đăng nhập thành công
            var tokenData = result.Data as TokenResult;
            var accessToken = tokenData?.AccessToken;

            if (!string.IsNullOrEmpty(accessToken))
            {
                // ✅ Gắn JWT accessToken vào cookie
                Response.Cookies.Append("jwt", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // để bảo mật hơn khi chạy https
                    SameSite = SameSiteMode.None, // nếu dùng frontend khác domain
                    Expires = DateTime.UtcNow.AddHours(1)
                });
            }

            return Ok(new
            {
                message = "Login success",
                token = result.Data
            });
        }

        // Endpoint chỉ admin có thể truy cập
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("Chỉ admin mới vào được!");
        }

        // Endpoint chỉ user có thể truy cập
        [Authorize(Roles = "User")]
        [HttpGet("user-only")]
        public IActionResult UserOnly()
        {
            return Ok("Chỉ user!");
        }

        // Kiểm tra quyền và thông tin user
        [Authorize]
        [HttpGet("check-role")]
        public IActionResult CheckRole()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var role = identity?.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { role });
        }

        [Authorize]
        [HttpGet("my-infor")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _mediator.Send(new GetUserById { UserId = userId });

            if (!result.Success) return Unauthorized();

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("user-infor")]
        public async Task<IActionResult> GetUserInfor(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new GetUserById
            {
                UserId = userId,
                CurrentUserId = currentUserId
            });

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Đăng xuất người dùng
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logout success" });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                var jwtToken = _jwtHelpers.Verify(token);
                if (jwtToken == null)
                    return BadRequest("Invalid or expired token.");

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return BadRequest("Invalid token content.");

                var result = await _mediator.Send(new VerifyUserCommand { UserId = userId });
                if (!result.Success)
                    return BadRequest(result.Message);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update-user-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserProfileCommand command)
        {
            
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }
            
            Console.WriteLine($"📋 Form data received:");
            if (Request.Form != null)
            {
                foreach (var formField in Request.Form)
                {
                    Console.WriteLine($"  {formField.Key}: {formField.Value}");
                }
            }
            
            // Add this validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                foreach (var error in errors)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value)}");
                }
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = errors
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"🔑 UserId from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"❌ Invalid token - no userId found");
                return Unauthorized(new { message = "Invalid token" });
            }
            
            command.UserId = userId;
            
            try
            {
                var result = await _mediator.Send(command);
                
                if (result.Success)
                {
                    try
                    {
                        // Lấy dữ liệu user đã cập nhật từ database
                        var updatedUser = await _mediator.Send(new GetUserById 
                        { 
                            UserId = userId,
                            CurrentUserId = userId 
                        });
                        
                        if (updatedUser.Success && updatedUser.Data != null)
                        {
                            // ✅ CAST VỀ ĐÚNG TYPE UserResponse
                            var userData = updatedUser.Data as UserResponse;
                            if (userData != null)
                            {

                                var finalResponse = new ApiResponse
                                {
                                    Success = true,
                                    Message = "Profile updated successfully.",
                                    Data = userData
                                };
                                
                                return Ok(finalResponse);
                            }
                            else
                            {
                                Console.WriteLine($"❌ Failed to cast updatedUser.Data to UserResponse. Data type: {updatedUser.Data?.GetType()?.Name}");
                                // ❌ KHÔNG FALLBACK - TRẢ VỀ LỖI
                                return StatusCode(500, new ApiResponse
                                {
                                    Success = false,
                                    Message = "Failed to cast user data to UserResponse"
                                });
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to retrieve updated user data. Success: {updatedUser.Success}, Data: {updatedUser.Data != null}");
                            // ❌ KHÔNG FALLBACK - TRẢ VỀ LỖI
                            return StatusCode(500, new ApiResponse
                            {
                                Success = false,
                                Message = "Failed to retrieve updated user data from database"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Exception occurred while retrieving updated user data: {ex.Message}");
                        // ❌ KHÔNG FALLBACK - TRẢ VỀ LỖI
                        return StatusCode(500, new ApiResponse
                        {
                            Success = false,
                            Message = $"Exception occurred: {ex.Message}"
                        });
                    }
                }
                
                // ❌ Nếu UpdateUserProfileCommand thất bại, trả về lỗi
                Console.WriteLine($"❌ UpdateUserProfileCommand failed - returning BadRequest");
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Exception occurred in UpdateUserProfile endpoint: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("verify-reset-token")]
        public IActionResult VerifyResetToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { success = false, message = "Token không hợp lệ" });

                var jwtToken = _jwtHelpers.Verify(token);
                if (jwtToken == null)
                    return BadRequest(new { success = false, message = "Token không hợp lệ hoặc đã hết hạn" });

                var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "Type")?.Value;
                if (tokenType != "PasswordReset")
                    return BadRequest(new { success = false, message = "Token không hợp lệ" });

                return Ok(new { success = true, message = "Token hợp lệ" });
            }
            catch
            {
                return BadRequest(new { success = false, message = "Token không hợp lệ" });
            }
        }

        [HttpGet("coin")]
        public async Task<IActionResult> GetUserCoin()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new GetUserCoin
            {
                UserId = userId
            });

            return Ok(result);
        }

        [HttpPut("update-to-admin")]
        public async Task<IActionResult> UpdateUserToAdmin(string userId)
        {
            var result = await _mediator.Send(new UpdateUserToAdminCommand { UserId = userId });
            return Ok(result);
        }

        [HttpGet("admin-id")]
        public async Task<IActionResult> GetAdminId()
        {
            var response = await _mediator.Send(new GetAdminId());
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUser query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        
        [HttpPut("ban-vs-unban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanStatus([FromBody] UpdateLockvsUnLockUserCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
