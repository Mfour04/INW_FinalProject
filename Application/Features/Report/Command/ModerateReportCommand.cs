using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Report.Command
{
    public class ModerateReportCommand : IRequest<ApiResponse>
    {
        public string? ReportId { get; set; }
        public ReportStatus Status { get; set; }
        public ModerationAction Action { get; set; }
        public string? ModeratorNote { get; set; }
        public long? SuspendUntilTicks { get; set; }
    }

    public class ModerateReportCommandHandler : IRequestHandler<ModerateReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IForumPostRepository _forumPostRepo;
        private readonly IForumCommentRepository _forumCommentRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUser;

        public ModerateReportCommandHandler(
            IReportRepository reportRepo,
            INovelRepository novelRepo,
            IChapterRepository chapterRepo,
            ICommentRepository commentRepo,
            IForumPostRepository forumPostRepo,
            IForumCommentRepository forumCommentRepo,
            IUserRepository userRepo,
            ICurrentUserService currentUser)
        {
            _reportRepo = reportRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _commentRepo = commentRepo;
            _forumPostRepo = forumPostRepo;
            _forumCommentRepo = forumCommentRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(ModerateReportCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAdmin())
                return Fail("You don't have permission to moderate reports.");

            if (string.IsNullOrWhiteSpace(request.ReportId))
                return Fail("Invalid report id.");

            var report = await _reportRepo.GetByIdAsync(request.ReportId);
            if (report == null)
                return Fail("Report not found.");

            if (report.status != ReportStatus.Pending)
                return Fail("This report has already been processed and cannot be updated.");

            if (request.Status == ReportStatus.Pending)
                return Fail("Cannot update report status to Pending.");

            if (request.Status != ReportStatus.Resolved &&
                request.Status != ReportStatus.Rejected &&
                request.Status != ReportStatus.Ignored)
                return Fail("Invalid target status. Must be Resolved, Rejected, or Ignored.");

            if (request.Status == ReportStatus.Resolved)
            {
                if (request.Action == ModerationAction.None)
                    return Fail("A moderation action is required when status is Resolved.");

                var ok = await ApplyModerationActionAsync(report, request.Action, request.SuspendUntilTicks);
                if (!ok) return Fail("Failed to apply moderation action on the resource.");
            }
            else
            {
                if (request.Action != ModerationAction.None)
                    return Fail("Moderation action must be None when status is Rejected or Ignored.");
            }

            ReportEntity updated = new()
            {
                status = request.Status,
                action = request.Action,
                moderator_id = _currentUser.UserId ?? string.Empty,
                moderator_note = (request.ModeratorNote ?? string.Empty).Trim(),
                moderated_at = TimeHelper.NowTicks
            };

            var success = await _reportRepo.UpdateAsync(request.ReportId, updated);
            if (!success)
                return Fail("Failed to update the report.");

            return new ApiResponse
            {
                Success = true,
                Message = "Report updated successfully."
            };
        }

        private ApiResponse Fail(string message) => new() { Success = false, Message = message };

        private async Task<bool> ApplyModerationActionAsync(ReportEntity r, ModerationAction action, long? suspendUntilTicks)
        {
            if (action == ModerationAction.None) return true;

            try
            {
                switch (r.scope)
                {
                    case ReportScope.Novel:
                        return await ModerateNovelAsync(r.novel_id, action);
                    case ReportScope.Chapter:
                        return await ModerateChapterAsync(r.chapter_id, action);
                    case ReportScope.Comment:
                        return await ModerateCommentAsync(r.comment_id, action);
                    case ReportScope.ForumPost:
                        return await ModerateForumPostAsync(r.forum_post_id, action);
                    case ReportScope.ForumComment:
                        return await ModerateForumCommentAsync(r.forum_comment_id, action);
                    case ReportScope.User: // NEW
                        return await ModerateUserAsync(r.target_user_id, action, suspendUntilTicks);
                    default: return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ModerateNovelAsync(string novelId, ModerationAction action)
        {
            switch (action)
            {
                case ModerationAction.HideResource:
                    await _novelRepo.UpdateLockStatusAsync(novelId, true);
                    return true;
                // case ModerationAction.DeleteResource:
                //     await _novelRepo.SoftDeleteAsync(novelId);
                //     return true;
                default:
                    return true; // các action khác hiện chưa có side-effect
            }
        }

        private async Task<bool> ModerateChapterAsync(string chapterId, ModerationAction action)
        {
            switch (action)
            {
                // case ModerationAction.HideResource:
                //     await _chapterRepo.UpdateLockChaptersStatus(chapterId, true);
                //     return true;
                // case ModerationAction.DeleteResource:
                //     await _chapterRepo.SoftDeleteAsync(chapterId);
                //     return true;
                default:
                    return true;
            }
        }

        private async Task<bool> ModerateCommentAsync(string commentId, ModerationAction action)
        {
            switch (action)
            {
                // case ModerationAction.HideResource:
                //     await _commentRepo.SetHiddenAsync(commentId, true);
                //     return true;
                case ModerationAction.DeleteResource:
                    await _commentRepo.DeleteAsync(commentId);
                    return true;
                default:
                    return true;
            }
        }

        private async Task<bool> ModerateForumPostAsync(string postId, ModerationAction action)
        {
            switch (action)
            {
                // case ModerationAction.HideResource:
                //     await _forumPostRepo.SetHiddenAsync(postId, true);
                //     return true;
                case ModerationAction.DeleteResource:
                    await _forumPostRepo.DeleteAsync(postId);
                    return true;
                default:
                    return true;
            }
        }

        private async Task<bool> ModerateForumCommentAsync(string forumCommentId, ModerationAction action)
        {
            switch (action)
            {
                // case ModerationAction.HideResource:
                //     await _forumCommentRepo.SetHiddenAsync(forumCommentId, true);
                //     return true;
                case ModerationAction.DeleteResource:
                    await _forumCommentRepo.DeleteAsync(forumCommentId);
                    return true;
                default:
                    return true;
            }
        }

        private async Task<bool> ModerateUserAsync(
           string userId,
           ModerationAction action,
           long? suspendUntilTicks)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;

            switch (action)
            {
                case ModerationAction.WarnUser:
                    // Tuỳ hệ thống log: nếu có bảng log moderation riêng, ghi lại:
                    // await _userRepo.AddModerationLogAsync(userId, moderatorId, "Warn", report.moderator_note, TimeHelper.NowTicks);
                    return true;

                case ModerationAction.SuspendUser:
                    {
                        // Default 72h
                        var now = TimeHelper.NowTicks;
                        var until = suspendUntilTicks.HasValue && suspendUntilTicks.Value > now
                            ? suspendUntilTicks.Value
                            : now + TimeSpan.TicksPerHour * 72;

                        await _userRepo.UpdateLockvsUnLockUser(userId, true, until);

                        return true;
                    }

                case ModerationAction.BanUser:
                    {
                        await _userRepo.UpdateLockvsUnLockUser(userId, true, 0);
                        return true;
                    }

                default:
                    // Action không áp dụng cho user (HideResource/DeleteResource...) => coi như no-op
                    return true;
            }
        }
    }
}
