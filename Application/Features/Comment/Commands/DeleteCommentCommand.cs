using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Comment.Commands
{
    public class DeleteCommentCommand : IRequest<ApiResponse>
    {
        public string CommentId { get; set; }
    }

    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notificationService;
        public DeleteCommentCommandHandler(
            ICommentRepository commentRepo,
            INovelRepository novelRepo,
            IChapterRepository chapterRepo,
            ICurrentUserService currentUser,
            INotificationService notificationService)
        {
            _commentRepo = commentRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _currentUser = currentUser;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var existComment = await _commentRepo.GetByIdAsync(request.CommentId);
            if (existComment == null)
                return Fail("Không tìm thấy bình luận.");

            bool isAdmin = _currentUser.IsAdmin();
            bool isOwner = _currentUser.UserId == existComment.user_id;

            if (!_currentUser.IsAdmin() && _currentUser.UserId != existComment.user_id)
                return Fail("Bạn không có quyền xóa bình luận này.");

            int totalDeleted = 1;

            if (string.IsNullOrWhiteSpace(existComment.parent_comment_id))
            {
                var replyIds = await _commentRepo.GetReplyIdsByParentIdAsync(existComment.id);

                if (replyIds.Any())
                {
                    await _commentRepo.DeleteManyAsync(replyIds);
                    totalDeleted += replyIds.Count;
                }
            }

            var deleted = await _commentRepo.DeleteAsync(request.CommentId);
            if (!deleted)
            {
                return Fail("Xóa bình luận thất bại.");
            }

            if (string.IsNullOrWhiteSpace(existComment.parent_comment_id))
            {
                await _commentRepo.DeleteRepliesByParentIdAsync(existComment.id);
            }

            if (!string.IsNullOrWhiteSpace(existComment.chapter_id))
                await _chapterRepo.DecrementCommentsAsync(existComment.chapter_id, totalDeleted);
            else if (!string.IsNullOrWhiteSpace(existComment.novel_id))
                await _novelRepo.DecrementCommentsAsync(existComment.novel_id, totalDeleted);

            // 🔔 Gửi thông báo nếu admin xóa bình luận của người khác
            if (isAdmin && !isOwner)
            {
                await _notificationService.SendNotificationToUsersAsync(
                    new[] { existComment.user_id },
                    "1 bình luận của bạn đã bị quản trị viên xóa vì vi phạm quy tắc cộng đồng.",
                    NotificationType.CommentDeleted
                );
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Xóa bình luận thành công"
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
