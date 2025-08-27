using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UnlikePostCommentCommand : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
    }

    public class UnlikePostCommentCommandHandler : IRequestHandler<UnlikePostCommentCommand, ApiResponse>
    {
        private readonly ICommentLikeRepository _commentLikeRepo;
        private readonly IForumCommentRepository _commentRepo;

        public UnlikePostCommentCommandHandler(
            ICommentLikeRepository commentLikeRepo,
            IForumCommentRepository commentRepo)
        {
            _commentLikeRepo = commentLikeRepo;
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse> Handle(UnlikePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CommentId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Thiếu trường bắt buộc: CommentId hoặc UserId.");

            var comment = await _commentRepo.GetByIdAsync(request.CommentId);
            if (comment == null)
                return Fail("Bình luận không tồn tại.");

            var hasLiked = await _commentLikeRepo.HasUserLikedCommentAsync(request.CommentId, request.UserId);
            if (!hasLiked)
                return Fail("Người dùng chưa thích bình luận này.");

            var isSuccess = await _commentLikeRepo.UnlikeCommentAsync(request.CommentId, request.UserId);
            if (!isSuccess)
                return Fail("Hủy thích bình luận thất bại.");

            return new ApiResponse
            {
                Success = true,
                Message = "Hủy thích thành công."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
