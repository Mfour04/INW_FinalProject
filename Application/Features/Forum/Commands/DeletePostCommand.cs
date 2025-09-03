using Application.Services.Implements;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class DeletePostCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
    }

    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;
        private readonly ICloudDinaryService _cloudDinaryService;
        private readonly IForumCommentRepository _commentRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public DeletePostCommandHandler(IForumPostRepository postRepo, ICloudDinaryService cloudDinaryService
            , IForumCommentRepository commentRepo, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _postRepo = postRepo;
            _cloudDinaryService = cloudDinaryService;
            _commentRepo = commentRepo;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Không tìm thấy bài đăng.");
            bool isAdmin = _currentUserService.IsAdmin();
            if (post.user_id != request.UserId && !isAdmin)
                return Fail("Người dùng không có quyền xóa bài đăng này.");

            var deleted = await _postRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Xóa bài đăng thất bại.");
            await _commentRepo.DeleteAllCommentByBlogId(request.Id);
            if (post.img_urls != null && post.img_urls.Any())
            {
                foreach (var imgUrl in post.img_urls)
                {
                    await _cloudDinaryService.DeleteImageAsync(imgUrl);
                }
            }

            if (isAdmin && post.user_id != request.UserId)
            {
                await _notificationService.SendNotificationToUsersAsync(
                    new[] { post.user_id },
                    "1 bài đăng của bạn trên cộng đồng đã bị quản trị viên xóa vì vi phạm tiêu chuẩn cộng đồng.",
                    NotificationType.ForumPostDeleted
                   );
            }


            return new ApiResponse
            {
                Success = true,
                Message = "Xóa bài đăng thành công."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}