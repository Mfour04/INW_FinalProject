using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalRHub
{
    public class NotificationHub: Hub
    {
        public async Task<bool> SendNotificationToUser(string userId, string message)
        {         
            try
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    Type = ToString()
                });
                return true;
            }
            catch
            {
                // Log lỗi nếu cần
                return false;
            }
        }
    }
}
