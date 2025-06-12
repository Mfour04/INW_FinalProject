using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string UserId { get; set; }
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
                return new ApiResponse { Success = false, Message = "Content cannot be empty." };

            var updated = await _postRepo.GetByIdAsync(request.Id);
            if (updated == null)
                return new ApiResponse { Success = false, Message = "Post not found." };

            if (updated.user_id != request.UserId)
                return new ApiResponse { Success = false, Message = "User are not allowed to edit this post." };

            updated.content = request.Content;
            updated.updated_at = DateTime.Now.Ticks;

            var isSuccess = await _postRepo.UpdateAsync(request.Id, updated);

            if (!isSuccess)
                return new ApiResponse { Success = false, Message = "Failed to update the post or post not found." };

            return new ApiResponse
            {
                Success = true,
                Message = "Post updated successfully.",
            };
        }
    }
}