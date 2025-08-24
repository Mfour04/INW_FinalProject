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
        public string ReportId { get; set; } = default!;
        public ReportStatus Status { get; set; }
        public ModerationAction Action { get; set; } = ModerationAction.None;
        public string? ModeratorNote { get; set; }
    }

    public class ModerateReportCommandHandler : IRequestHandler<ModerateReportCommand, ApiResponse>
    {
        private readonly IReportRepository _reportRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IForumPostRepository _forumPostRepo;
        private readonly IForumCommentRepository _forumCommentRepo;
        private readonly ICurrentUserService _currentUser;

        public ModerateReportCommandHandler(
            IReportRepository reportRepo,
            INovelRepository novelRepo,
            IChapterRepository chapterRepo,
            ICommentRepository commentRepo,
            IForumPostRepository forumPostRepo,
            IForumCommentRepository forumCommentRepo,
            ICurrentUserService currentUser)
        {
            _reportRepo = reportRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _commentRepo = commentRepo;
            _forumPostRepo = forumPostRepo;
            _forumCommentRepo = forumCommentRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(ModerateReportCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAdmin())
                return Fail("You don't have permission to moderate reports.");

            var report = await _reportRepo.GetByIdAsync(request.ReportId);
            if (report == null)
                return Fail("Report not found.");

            if (report.status != ReportStatus.Pending)
                return Fail("This report has already been processed.");

            if (request.Status == ReportStatus.Resolved)
            {
                var ok = await ApplyModerationActionAsync(report, request.Action);
                if (!ok)
                    return Fail("Failed to apply moderation action on the resource.");
            }

            report.status = request.Status;
            report.action = request.Action;
            report.moderator_id = _currentUser.UserId ?? string.Empty;
            report.moderator_note = (request.ModeratorNote ?? string.Empty).Trim();
            report.moderated_at = TimeHelper.NowTicks;
            report.updated_at = TimeHelper.NowTicks;

            await _reportRepo.UpdateAsync(report);

            return new ApiResponse
            {
                Success = true,
                Message = "Report updated successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        private async Task<bool> ApplyModerationActionAsync(ReportEntity r, ModerationAction action)
        {
            if (action == ModerationAction.None) return true;

            try
            {
                switch (r.scope)
                {
                    case ReportScope.Novel: return await ModerateNovelAsync(r.novel_id, action);
                    case ReportScope.Chapter: return await ModerateChapterAsync(r.chapter_id, action);
                    case ReportScope.Comment: return await ModerateCommentAsync(r.comment_id, action);
                    case ReportScope.ForumPost: return await ModerateForumPostAsync(r.forum_post_id, action);
                    case ReportScope.ForumComment: return await ModerateForumCommentAsync(r.forum_comment_id, action);
                    default: return false;
                }
            }
            catch { return false; }
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
                default: return true;
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
                default: return true;
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
                default: return true;
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
                default: return true;
            }
        }

        private async Task<bool> ModerateForumCommentAsync(string forumCommentId, ModerationAction action)
        {
            switch (action)
            {
                // case ModerationAction.HideResource:
                //     await _forumCommentRepo.DeleteAsync(forumCommentId);
                //     return true;
                case ModerationAction.DeleteResource:
                    await _forumCommentRepo.DeleteAsync(forumCommentId);
                    return true;
                default: return true;
            }
        }
    }
}