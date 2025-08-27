using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class LikePostCommentCommand : IRequest<ApiResponse>
    {
        public string CommentId { get; set; }
        public string UserId { get; set; }
    }

    public class LikePostCommentCommandHandler : IRequestHandler<LikePostCommentCommand, ApiResponse>
    {
        private readonly ICommentLikeRepository _commentLikeRepo;
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepo;
        public LikePostCommentCommandHandler(
            ICommentLikeRepository commentLikeRepo,
            IForumCommentRepository postCommentRepo,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _commentLikeRepo = commentLikeRepo;
            _postCommentRepo = postCommentRepo;
            _notificationService = notificationService;
            _userRepo = userRepository;
        }

        public async Task<ApiResponse> Handle(LikePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CommentId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Thiếu trường bắt buộc: CommentId hoặc UserId.");

            var targetComment = await _postCommentRepo.GetByIdAsync(request.CommentId);
            if (targetComment == null)
                return Fail("Bình luận không tồn tại.");

            var hasLiked = await _commentLikeRepo.HasUserLikedCommentAsync(request.CommentId, request.UserId);
            if (hasLiked)
                return Fail("Người dùng đã thích bình luận này.");

            var like = new CommentLikeEntity
            {
                id = SystemHelper.RandomId(),
                comment_id = request.CommentId,
                user_id = request.UserId,
                type = CommentType.Forum,
                like_at = TimeHelper.NowTicks
            };

            await _commentLikeRepo.LikeCommentAsync(like);

            // 📌 Gửi thông báo cho tác giả comment
            if (!string.IsNullOrWhiteSpace(targetComment.user_id) && targetComment.user_id != request.UserId)
            {
                var liker = await _userRepo.GetById(request.UserId);
                string message = $"{liker.displayname} đã thích bình luận của bạn.";
                await _notificationService.SendNotificationToUsersAsync(
                    new[] { targetComment.user_id },
                    message,
                    NotificationType.LikePostComment
                );
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Thích bình luận bài viết thành công.",
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
