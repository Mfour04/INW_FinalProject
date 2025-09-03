using Application.Auth.Commands;
using Application.Features.Novel.Queries;
using Application.Features.User.Feature;
using Application.Features.User.Queries;
using Application.Services.Interfaces;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Response.User;
using Shared.Helpers;
using Shared.SystemHelpers.TokenGenerate;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly JwtHelpers _jwtHelpers;
        private readonly IConfiguration _config;

        public UsersController(IMediator mediator, JwtHelpers jwtHelpers
            , IConfiguration config)
        {
            _mediator = mediator;
            _jwtHelpers = jwtHelpers;
            _config = config;
        }

        public class GoogleLoginRequest
        {
            public string AccessToken { get; set; }
        }
        public class GoogleUserInfo
        {
            public string Sub { get; set; }
            public string Name { get; set; }

            [JsonPropertyName("given_name")]
            public string GivenName { get; set; }

            [JsonPropertyName("family_name")]
            public string FamilyName { get; set; }

            [JsonPropertyName("picture")]
            public string Picture { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("email_verified")]
            public bool EmailVerified { get; set; }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AccessToken);

                var response = await httpClient.GetStringAsync("https://www.googleapis.com/oauth2/v3/userinfo");

                // 🔍 Log thử response từ Google
                Console.WriteLine("Google userinfo response: " + response);

                var userInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(
                    response,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
                {
                    return Unauthorized(new
                    {
                        Message = "Token truy cập Google không hợp lệ",
                        Error = "Không tìm thấy thông tin người dùng",
                        RawResponse = response   // 👈 thêm raw response để debug
                    });
                }

                var command = new LoginGoogleCommand
                {
                    Email = userInfo.Email,
                    Name = userInfo.Name ?? userInfo.GivenName ?? userInfo.Email,
                    AvatarUrl = userInfo.Picture
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GoogleLogin Error] {ex}");
                return Unauthorized(new
                {
                    Message = "Token truy cập Google không hợp lệ",
                    Error = ex.Message,
                    Stack = ex.StackTrace
                });
            }
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

            TokenResult? tokenData = null;
            string message;

            if (!result.Success || result.Data == null)
            {
                // Trường hợp login fail
                message = "Sai tài khoản hoặc mật khẩu";
            }
            else
            {
                // Trường hợp login thành công
                tokenData = result.Data as TokenResult;
                message = "Đăng nhập thành công";

                // Gắn JWT vào cookie nếu có accessToken
                var accessToken = tokenData?.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Response.Cookies.Append("jwt", accessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = TimeHelper.NowVN.AddHours(1)
                    });
                }
            }

            // Trả về ApiResponse với token
            return Ok(new
            {
                success = result.Success,
                message,
                token = tokenData
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
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var result = await _mediator.Send(new GetUserById { UserId = userId, CurrentUserId = userId });

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
            return Ok(new { message = "Đăng xuất thành công" });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                var jwtToken = _jwtHelpers.Verify(token);
                if (jwtToken == null)
                    return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

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
            // Add this validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = errors
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            command.UserId = userId;
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
