using Application.Services.Interfaces;
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

        public DeletePostCommandHandler(IForumPostRepository postRepo, ICloudDinaryService cloudDinaryService)
        {
            _postRepo = postRepo;
            _cloudDinaryService = cloudDinaryService;
        }

        public async Task<ApiResponse> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Không tìm thấy bài đăng.");

            if (post.user_id != request.UserId)
                return Fail("Người dùng không có quyền xóa bài đăng này.");

            var deleted = await _postRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Xóa bài đăng thất bại.");

            if (post.img_urls != null && post.img_urls.Any())
            {
                foreach (var imgUrl in post.img_urls)
                {
                    await _cloudDinaryService.DeleteImageAsync(imgUrl);
                }
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