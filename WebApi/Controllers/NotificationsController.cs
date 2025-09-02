using Application.Features.Notification.Commands;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            await _mediator.Send(command.SenderId = userId);
            return Ok(new { success = true, message = "Notification sent." });
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationsAsReadCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
