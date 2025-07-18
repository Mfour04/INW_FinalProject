namespace Shared.Contracts.Response.Forum
{
    public class CreatePostCommentResponse
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public string ParentCommentId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public long CreatedAt { get; set; }
    }
}