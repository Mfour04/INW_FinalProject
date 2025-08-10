using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Services.Implements
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ??connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            Console.WriteLine($"SignalR UserIdProvider: UserId = {userId}");
            return userId;
        }

    }
}
