using Application.Features.Notification.Command;
using Application.Features.Notification.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/notifications")]
        [Authorize]
        public async Task<IActionResult> GetUsesrNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _mediator.Send(new GetNotificationByUserId { UserId = userId});
            return Ok(result);
        }

        [HttpPost("send-notify")]
        public async Task<IActionResult> SendNotifyUser([FromBody] SendNotificationToUserCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { success = true, message = "Notification sent." });
        }
    }
}
