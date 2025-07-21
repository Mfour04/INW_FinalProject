using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.SignalRHub;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string message, NotificationType type)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                message,
                type,
                create_at = DateTime.UtcNow.Ticks
            });
        }   
    }
}
