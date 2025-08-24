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
        public string NovelId { get; set; } = default!;
    }

    // ==== Chapter ====
    public sealed class ChapterReportResponse : BaseReportResponse
    {
        public string NovelId { get; set; } = default!;
        public string ChapterId { get; set; } = default!;
    }

    // ==== Comment (thuộc novel/chapter) ====
    public sealed class CommentReportResponse : BaseReportResponse
    {
        public string? NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string CommentId { get; set; } = default!;
    }

    // ==== Forum Post ====
    public sealed class ForumPostReportResponse : BaseReportResponse
    {
        public string ForumPostId { get; set; } = default!;
    }

    // ==== Forum Comment ====
    public sealed class ForumCommentReportResponse : BaseReportResponse
    {
        public string ForumCommentId { get; set; } = default!;
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
