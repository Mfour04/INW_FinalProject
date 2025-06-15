using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class DeletePostCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
        public string UserId { get; set; }
    }

    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;

        public DeletePostCommandHandler(IForumPostRepository postRepo)
        {
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Post not found.");

            if (post.user_id != request.UserId)
                return Fail("User is not allowed to delete this post.");

            var deleted = await _postRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Failed to delete the post.");

            return new ApiResponse
            {
                Success = true,
                Message = "Post deleted successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}