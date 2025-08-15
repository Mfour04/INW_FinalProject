using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Response.User
{
    public class UpdateUserProfileReponse
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
    }
}
