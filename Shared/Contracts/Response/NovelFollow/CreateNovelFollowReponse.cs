using Domain.Enums;

namespace Shared.Contracts.Response.NovelFollow
{
    public class CreateNovelFollowReponse
    {
        public string NovelFollowId { get; set; }
        public string NovelId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsNotification { get; set; }
        public NovelFollowReadingStatus ReadingStatus { get; set; }
        public long FollowedAt { get; set; }
    }
}
