using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
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
            ForumPostEntity updated = new()
            {
                content = request.Content,
                updated_at = DateTime.Now.Ticks
            };

            var isSuccess = await _postRepo.UpdateForumPostAsync(request.Id, updated);

            if (!isSuccess)
                return ApiResponse.Fail("Cập nhật bài viết thất bại hoặc không tìm thấy.");

            return ApiResponse.Ok("Cập nhật bài viết thành công.");
        }
    }
}