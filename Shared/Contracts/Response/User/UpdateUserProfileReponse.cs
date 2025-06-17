using Microsoft.AspNetCore.Http;

namespace Shared.Contracts.Response.User
{
    public class UpdateUserProfileReponse
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public IFormFile? AvataUrl { get; set; }
        public string Bio { get; set; }
        public List<string> BadgeId { get; set; } = new();
    }
}
