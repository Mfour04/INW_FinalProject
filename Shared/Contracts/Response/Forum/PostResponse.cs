namespace Shared.Contracts.Response.Forum
{
    public class PostResponse
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public List<string> ImgUrls { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public bool IsLiked { get; set; }
        public ForumPostAuthorResponse Author { get; set; }
    }

    public class ForumPostAuthorResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
    }
}