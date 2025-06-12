namespace Shared.Contracts.Response.Forum
{
    public class PostCommentResponse
    {
        public string Id { get; set; }
		public string PostId { get; set; }
		public ForumPostCommentAuthorResponse Author { get; set; }
        public string Content { get; set; }
        public string ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }

    public class ForumPostCommentAuthorResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
    }
}