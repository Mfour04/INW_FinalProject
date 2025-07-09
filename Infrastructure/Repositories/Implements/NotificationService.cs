using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.SignalRHub;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implements
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
