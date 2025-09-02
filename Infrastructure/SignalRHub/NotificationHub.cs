using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalRHub
{
    public class NotificationHub: Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"UserIdentifier = {Context.UserIdentifier}");
            foreach (var claim in Context.User.Claims)
            {
                Console.WriteLine($"{claim.Type} = {claim.Value}");
            }
            await base.OnConnectedAsync();
        }
    }
}
