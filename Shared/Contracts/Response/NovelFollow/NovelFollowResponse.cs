namespace Shared.Contracts.Response.Follow
{
    public class NovelFollowResponse
    {
        public string NovelFollowId { get; set; }
        public string NovelId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public long FollowedAt { get; set; }
    }
    // <summary>
    //Lấy danh sách user đang follow một novel
    //</sumary>
    public class NovelFollowerUserInfoResponse
    {
        public string FollowerId { get; set; }           // ID của bản ghi follow
        public string UserId { get; set; }               // ID của người dùng follow
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public long FollowedAt { get; set; }
    }

    public class GetNovelFollowersResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string NovelImage { get; set; }
        public string NovelBanner { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int Price { get; set; }
        public int TotalChapters { get; set; }
        public int TotalFollowers { get; set; }
        public List<NovelFollowerUserInfoResponse> Followers { get; set; } = new();
    }
    // <summary>
    //Lấy danh sách novel mà user đang follow
    //</sumary>
    public class UserFollowingNovelInfoResponse
    {
        public string FollowId { get; set; }         // id của bản ghi follow
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string NovelImage { get; set; }
        public string NovelBanner { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsPaid { get; set; }
        public int Price { get; set; }
        public double RatingAvg { get; set; }
        public int Followers { get; set; }
        public int TotalChapters { get; set; }
        public long FollowedAt { get; set; }
    }


    public class GetUserFollowedNovelsResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public int TotalFollowing { get; set; }
        public List<UserFollowingNovelInfoResponse> FollowedNovels { get; set; } = new();
    }

}
