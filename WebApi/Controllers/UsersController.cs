using Application.Auth.Commands;
using Application.Features.User.Feature;
using Application.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response.User;
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
            var email = identity?.FindFirst("Email")?.Value;

            return Ok(new { role, email });
        }

        [Authorize]
        [HttpGet("user-infor")]
        public async Task<IActionResult> GetUserInfo()
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

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized(new ApiResponse
            //     {
            //         Success = false,
            //         Message = "User not authenticated."
            //     });

            var result = await _mediator.Send(new GetUserCoin
            {
                UserId = "user_002"
            });

            return Ok(result);
        }

    }
}
