using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class DeletePostCommentCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _commentRepo;
        private readonly IForumPostRepository _postRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        public DeletePostCommentCommandHandler(
            IForumCommentRepository commentRepo,
            IForumPostRepository postRepo,
            ICurrentUserService currentUser,
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _commentRepo = commentRepo;
            _postRepo = postRepo;
            _currentUser = currentUser;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
        {
            var validation = await ValidateCommand(request.Id);
            if (!validation.IsValid)
                return validation.FailResponse!;

            var comment = validation.Comment!;

            var replyIds = await _commentRepo.GetReplyIdsByCommentIdAsync(comment.id);

            if (replyIds.Any())
                await _commentRepo.DeleteManyAsync(replyIds);

            var deleted = await _commentRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Không xóa được bình luận!");

            // Giảm comment count cho post
            string postId = comment.post_id;

            // Nếu đây là reply, tìm post_id từ parent comment
            if (string.IsNullOrWhiteSpace(postId) && !string.IsNullOrWhiteSpace(comment.parent_comment_id))
            {
                var parentComment = await _commentRepo.GetByIdAsync(comment.parent_comment_id);
                if (parentComment != null)
                {
                    postId = parentComment.post_id;
                }
            }

            if (!string.IsNullOrWhiteSpace(postId))
            {
                var totalDeleted = 1 + replyIds.Count;
                await _postRepo.DecrementCommentsAsync(postId, totalDeleted);
            }

            if (_currentUser.IsAdmin() && comment.user_id != _currentUser.UserId)
            {
                await _notificationService.SendNotificationToUsersAsync(
                    new[] { comment.user_id },
                    "1 bình luận của bạn trong một bài đăng trên diễn đàn đã bị quản trị viên xóa vì vi phạm tiêu chuẩn cộng đồng.",
                    NotificationType.ForumCommentDeleted
                    );
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Bình luận đã được xóa thành công."
            };
        }

        private async Task<(bool IsValid, ApiResponse? FailResponse, ForumCommentEntity? Comment)> ValidateCommand(string commentId)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return (false, Fail("Không tìm thấy bình luận."), null);

            if (!_currentUser.IsAdmin() && comment.user_id != _currentUser.UserId)
                return (false, Fail("Người dùng không được phép xóa bình luận này."), null);

            return (true, null, comment);
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}