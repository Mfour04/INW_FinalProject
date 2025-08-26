using Domain.Enums;

namespace Shared.Contracts.Response.Report
{
    public abstract class BaseReportResponse
    {
        public string Id { get; set; } = default!;
        public ReportScope Scope { get; set; }

        public UserResponse Reporter { get; set; } = default!;
        public ReportReason Reason { get; set; }
        public string? Message { get; set; }

        public ReportStatus Status { get; set; }
        public ModerationAction Action { get; set; }

        public UserResponse Moderator { get; set; }
        public string? ModeratorNote { get; set; }
        public long ModeratedAt { get; set; }

        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }

        public class UserResponse
        {
            public string Id { get; set; } = default!;
            public string? Username { get; set; }
            public string? DisplayName { get; set; }
            public string? AvatarUrl { get; set; }
        }
    }

    // ==== Novel ====
    public sealed class NovelReportResponse : BaseReportResponse
    {
        public string? NovelId { get; set; }
        public string? NovelTitle { get; set; }
    }

    // ==== Chapter ====
    public sealed class ChapterReportResponse : BaseReportResponse
    {
        public string? NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public string? NovelTitle { get; set; }
    }

    // ==== Comment (thuộc novel/chapter) ====
    public sealed class CommentReportResponse : BaseReportResponse
    {
        public string? NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string CommentId { get; set; } = default!;
        public UserResponse? CommentAuthor { get; set; }
    }

    // ==== Forum Post ====
    public sealed class ForumPostReportResponse : BaseReportResponse
    {
        public string? ForumPostId { get; set; }
        public UserResponse? ForumPostAuthor { get; set; }
    }

    // ==== Forum Comment ====
    public sealed class ForumCommentReportResponse : BaseReportResponse
    {
        public string? ForumCommentId { get; set; }
        public UserResponse? ForumCommentAuthor { get; set; }
    }

    // ==== User ====
    public sealed class UserReportResponse : BaseReportResponse
    {
        public string? TargetUserId { get; set; }
        public UserResponse? TargetUser { get; set; }
    }

    // ==== Brief (dùng cho list) – gọn nhẹ ====
    public sealed class ReportBriefResponse
    {
        public string Id { get; set; } = default!;
        public ReportScope Scope { get; set; }
        public ReportReason Reason { get; set; }
        public ReportStatus Status { get; set; }
        public ModerationAction Action { get; set; }
        public long CreatedAt { get; set; }
    }
}
