namespace Shared.Contracts.Response.Follow
{
    public class NovelFollowResponse
    {
        public string NovelFollowId { get; set; }
        public string NovelId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public long FollowedAt { get; set; }
    }
}
