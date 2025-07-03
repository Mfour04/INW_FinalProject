using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalRHub
{
    public class NotificationHub: Hub
    {
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
