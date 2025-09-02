using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Report.Command
{
    public class CreateReportCommand : IRequest<ApiResponse>
    {
        public ReportScope Scope { get; set; }
        public ReportReason Reason { get; set; }

        public string? NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string? CommentId { get; set; }
        public string? ForumPostId { get; set; }
        public string? ForumCommentId { get; set; }
        public string? TargetUserId { get; set; }

        public string? Message { get; set; }
    }

    public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IForumPostRepository _forumPostRepo;
        private readonly IForumCommentRepository _forumCommentRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;

        // === Anti-spam ===
        private const int COOLDOWN_SECONDS = 120; // 2 phút giữa 2 report
        private const int DUP_WINDOW_SECONDS = 600; // 10 phút không trùng target + reason (Pending)
        private const int DAILY_LIMIT = 10;  // tối đa 10 report / 24h / user

        private static long SecToTicks(int s) => (long)s * TimeSpan.TicksPerSecond;

        public CreateReportCommandHandler(
           IReportRepository reportRepo,
           INovelRepository novelRepo,
           IChapterRepository chapterRepo,
           ICommentRepository commentRepo,
           IForumPostRepository forumPostRepo,
           IForumCommentRepository forumCommentRepo,
           ICurrentUserService currentUser,
           INotificationService notificationService,
           IUserRepository userRepo)
        {
            _reportRepo = reportRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _commentRepo = commentRepo;
            _forumPostRepo = forumPostRepo;
            _forumCommentRepo = forumCommentRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(CreateReportCommand req, CancellationToken ct)
        {
            var reporterId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(reporterId))
                return Fail("Bạn phải đăng nhập để gửi báo cáo.");

            var targetId = GetTargetIdByScope(req.Scope, req);
            if (string.IsNullOrWhiteSpace(targetId))
                return Fail($"Thiếu ID đối tượng cho phạm vi '{req.Scope}'.");

            if (req.Scope == ReportScope.User && string.Equals(targetId, reporterId, StringComparison.Ordinal))
                return Fail("Bạn không thể báo cáo chính mình.");

            var now = TimeHelper.NowTicks;

            // 1) Thời gian chờ giữa các báo cáo
            var cooldownFrom = now - SecToTicks(COOLDOWN_SECONDS);
            var cooldownCount = await _reportRepo.CountByReporterAsync(reporterId!, cooldownFrom);
            if (cooldownCount > 0)
                return Fail($"Vui lòng chờ {COOLDOWN_SECONDS} giây trước khi gửi báo cáo tiếp theo.");

            // 2) Giới hạn báo cáo trong ngày
            var dayFrom = now - TimeSpan.TicksPerDay;
            var dailyCount = await _reportRepo.CountByReporterAsync(reporterId!, dayFrom);
            if (dailyCount >= DAILY_LIMIT)
                return Fail("Bạn đã đạt giới hạn báo cáo trong ngày.");

            // 3) Duplicate window: cùng target + reason trong 10 phút và còn Pending
            var dupFrom = now - SecToTicks(DUP_WINDOW_SECONDS);
            var duplicate = await _reportRepo.ExistsAsync(
                reporterId: reporterId!,
                scope: req.Scope,
                novelId: req.NovelId,
                chapterId: req.ChapterId,
                commentId: req.CommentId,
                forumPostId: req.ForumPostId,
                forumCommentId: req.ForumCommentId,
                targetUserId: req.TargetUserId,
                reason: req.Reason,
                status: ReportStatus.Pending,
                fromTicks: dupFrom
            );
            if (duplicate)
                return Fail("Bạn đã báo cáo mục này gần đây. Chúng tôi đang xem xét.");

            var (resId, parentNovelId) = await ValidateAndExtractResourceAsync(req.Scope, req);
            if (string.IsNullOrEmpty(resId))
                return Fail("Không tìm thấy tài nguyên hoặc thông tin không khớp.");

            var report = new ReportEntity
            {
                id = SystemHelper.RandomId(),
                scope = req.Scope,

                novel_id = parentNovelId,
                chapter_id = req.ChapterId ?? "",
                comment_id = req.CommentId ?? "",
                forum_post_id = req.ForumPostId ?? "",
                forum_comment_id = req.ForumCommentId ?? "",
                target_user_id = (req.TargetUserId ?? "") ?? "",

                reporter_id = reporterId!,
                reason = req.Reason,
                message = (req.Message ?? string.Empty).Trim(),
                status = ReportStatus.Pending,

                action = ModerationAction.None,
                moderator_id = "",
                moderator_note = "",
                moderated_at = 0,

                created_at = now,
            };

            await _reportRepo.CreateAsync(report);

            var admins = await _userRepo.GetManyAdmin();
            var adminIds = admins.Select(a => a.id).ToArray();
            var reporter = await _userRepo.GetById(reporterId!);
            var reportMessage = string.IsNullOrWhiteSpace(req.Message)
                ? "(Không có nội dung kèm theo)"
                : req.Message.Trim();
            var reasonText = req.Reason.ToString();
            await _notificationService.SendNotificationToUsersAsync(
                adminIds,
                $"{reporter.displayname} đã tạo 1 báo cáo: {reasonText}. Nội dung: {reportMessage}.",
                NotificationType.CreateReport
            );

            return new ApiResponse
            {
                Success = true,
                Message = "Report submitted. Thank you!"
            };
        }

        private static ApiResponse Fail(string message) => new() { Success = false, Message = message };

        private static string? GetTargetIdByScope(ReportScope scope, CreateReportCommand r) =>
            scope switch
            {
                ReportScope.Novel => r.NovelId,
                ReportScope.Chapter => r.ChapterId,
                ReportScope.Comment => r.CommentId,
                ReportScope.ForumPost => r.ForumPostId,
                ReportScope.ForumComment => r.ForumCommentId,
                ReportScope.User => r.TargetUserId,
                _ => null
            };

        private async Task<(string resId, string parentNovelId)> ValidateAndExtractResourceAsync(ReportScope scope, CreateReportCommand req)
        {
            switch (scope)
            {
                case ReportScope.Novel:
                    {
                        var novel = await _novelRepo.GetByNovelIdAsync(req.NovelId!);
                        return novel == null ? ("", "") : (req.NovelId!, req.NovelId!);
                    }

                case ReportScope.Chapter:
                    {
                        var chap = await _chapterRepo.GetByIdAsync(req.ChapterId!);
                        if (chap == null) return ("", "");

                        var chapterNovelId = chap.novel_id;
                        if (!string.IsNullOrWhiteSpace(req.NovelId) &&
                            !string.Equals(chapterNovelId, req.NovelId, StringComparison.Ordinal))
                            return ("", "");

                        return (req.ChapterId!, chapterNovelId ?? req.NovelId ?? "");
                    }

                case ReportScope.Comment:
                    {
                        var cmt = await _commentRepo.GetByIdAsync(req.CommentId!);
                        if (cmt == null) return ("", "");

                        string parentNovelId = "";
                        if (!string.IsNullOrWhiteSpace(cmt.chapter_id))
                        {
                            var chap = await _chapterRepo.GetByIdAsync(cmt.chapter_id);
                            parentNovelId = chap?.novel_id ?? "";
                        }
                        else if (!string.IsNullOrWhiteSpace(cmt.novel_id))
                        {
                            parentNovelId = cmt.novel_id;
                        }
                        else
                        {
                            parentNovelId = req.NovelId ?? "";
                        }

                        if (!string.IsNullOrWhiteSpace(req.NovelId) &&
                            !string.IsNullOrWhiteSpace(parentNovelId) &&
                            !string.Equals(parentNovelId, req.NovelId, StringComparison.Ordinal))
                            return ("", "");

                        if (!string.IsNullOrWhiteSpace(req.ChapterId) &&
                            !string.IsNullOrWhiteSpace(cmt.chapter_id) &&
                            !string.Equals(cmt.chapter_id, req.ChapterId, StringComparison.Ordinal))
                            return ("", "");

                        return (req.CommentId!, parentNovelId);
                    }

                case ReportScope.ForumPost:
                    {
                        var post = await _forumPostRepo.GetByIdAsync(req.ForumPostId!);
                        return post == null ? ("", "") : (req.ForumPostId!, "");
                    }

                case ReportScope.ForumComment:
                    {
                        var fc = await _forumCommentRepo.GetByIdAsync(req.ForumCommentId!);
                        return fc == null ? ("", "") : (req.ForumCommentId!, "");
                    }
                case ReportScope.User:
                    {
                        var user = await _userRepo.GetById(req.TargetUserId!);
                        return user == null ? ("", "") : (req.TargetUserId!, "");
                    }

                default:
                    return ("", "");
            }
        }
    }
}
