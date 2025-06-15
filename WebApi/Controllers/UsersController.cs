using Application.Auth.Commands;
using Application.Features.User.Feature;
using Application.Features.User.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            Response.Cookies.Append("jwt", result.Data.ToString(), new CookieOptions
            {
                HttpOnly = true
            });

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

        // Lấy thông tin user từ token JWT
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                var token = _jwtHelpers.Verify(jwt);

                var userId = token.Issuer;
                var result = await _mediator.Send(new GetUserById { UserId = userId });

                if (!result.Success) return Unauthorized();

                return Ok(result.Data);
            }
            catch
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
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
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command)
        {
            var result = _mediator.Send(command);
            return Ok(result);
        }
    }
}
