using Microsoft.AspNetCore.Http;
using static Domain.Entities.UserEntity;

namespace Shared.Contracts.Response.User
{
    public class UpdateUserProfileReponse
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string AvataUrl { get; set; }
        public string CoverUrl { get; set; }    
        public string Bio { get; set; }
        public List<string> BadgeId { get; set; } = new();
        public List<TagName> FavouriteType { get; set; } = new();
    }
}
