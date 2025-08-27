using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UnlikePostCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
    }

    public class UnlikePostCommandHandler : IRequestHandler<UnlikePostCommand, ApiResponse>
    {
        private readonly IForumPostLikeRepository _postLikeRepo;
        private readonly IForumPostRepository _postRepo;

        public UnlikePostCommandHandler(
            IForumPostLikeRepository postLikeRepo,
            IForumPostRepository postRepo)
        {
            _postLikeRepo = postLikeRepo;
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PostId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Thiếu trường bắt buộc: PostId hoặc UserId.");

            var post = await _postRepo.GetByIdAsync(request.PostId);
            if (post == null)
                return Fail("Bài đăng không tồn tại.");

            var hasLiked = await _postLikeRepo.HasUserLikedPostAsync(request.PostId, request.UserId);
            if (!hasLiked)
                return Fail("Người dùng chưa thích bài đăng này.");

            var isSuccess = await _postLikeRepo.UnlikePostAsync(request.PostId, request.UserId);
            if (!isSuccess)
                return Fail("Hủy thích bài đăng thất bại.");

            await _postRepo.DecrementLikesAsync(request.PostId);

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
