using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
    }

    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;

        public UpdatePostCommandHandler(IForumPostRepository postRepo)
        {
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return Fail("Nội dung không được để trống.");

            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Không tìm thấy bài đăng.");

            if (post.user_id != request.UserId)
                return Fail("Bạn không có quyền chỉnh sửa bài đăng này.");

            ForumPostEntity updated = new()
            {
                content = request.Content
            };

            var success = await _postRepo.UpdateAsync(request.Id, updated);
            if (!success)
                return Fail("Cập nhật bài đăng thất bại.");

            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật bài đăng thành công."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
