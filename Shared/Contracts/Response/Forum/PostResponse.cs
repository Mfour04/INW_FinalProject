namespace Shared.Contracts.Response.Forum
{
    public abstract class BasePostResponse
    {
        public string Id { get; set; }
        public PostAuthorResponse Author { get; set; }
        public string Content { get; set; }
        public List<string> ImgUrls { get; set; }
        public long CreatedAt { get; set; }

        public class PostAuthorResponse
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Avatar { get; set; }
            public string DisplayName { get; set; }
        }
    }

    public class PostResponse : BasePostResponse
    {
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public long UpdatedAt { get; set; }

    }

    public class PostCreatedResponse : BasePostResponse { }
}