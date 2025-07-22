namespace Shared.Contracts.Response.Comment
{
    public abstract class BaseCommentResponse
    {
        public string Id { get; set; }
        public UserInfo Author { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string Content { get; set; }
        public long CreatedAt { get; set; }

        public class UserInfo
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
        }
    }

    public class CommentResponse : BaseCommentResponse
    {
        public long UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
    }

    public class CommentReplyResponse : BaseCommentResponse
    {
        public long UpdatedAt { get; set; }
        public int LikeCount { get; set; }
        public string ParentCommentId { get; set; }
    }

    public class CreateCommentResponse : BaseCommentResponse
    {
        public string ParentCommentId { get; set; }
        public SignalRResult SignalR { get; set; }

        public class SignalRResult
        {
            public bool Sent { get; set; }
            public string NotificationType { get; set; }
        }
    }

    public class UpdateCommentResponse
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public long UpdatedAt { get; set; }
    }
}
