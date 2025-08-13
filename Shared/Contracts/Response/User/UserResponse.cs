using Shared.Contracts.Response.Tag;
using static Domain.Entities.UserEntity;

namespace Shared.Contracts.Response.User
{
    public class UserResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string CoverUrl { get; set; }
        public string Bio { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBanned { get; set; }
        public long BannedUntil { get; set; }
        public int Coin { get; set; }
        public int BlockCoin { get; set; }
        public int NovelFollowCount { get; set; }
        public List<string> BadgeId { get; set; } = new();
        public long LastLogin { get; set; }
        public List<TagListResponse> FavouriteType { get; set; } = new();
        public long CreateAt { get; set; }
        public long UpdateAt { get; set; }
    }
}
