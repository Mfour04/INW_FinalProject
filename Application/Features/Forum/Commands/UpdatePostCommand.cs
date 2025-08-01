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
                return Fail("Content cannot be empty.");

            var post = await _postRepo.GetByIdAsync(request.Id);
            if (post == null)
                return Fail("Post not found.");

            if (post.user_id != request.UserId)
                return Fail("You are not allowed to edit this post.");

            ForumPostEntity updated = new()
            {
                content = request.Content
            };

            var success = await _postRepo.UpdateAsync(request.Id, updated);
            if (!success)
                return Fail("Failed to update the post.");

            return new ApiResponse
            {
                Success = true,
                Message = "Post updated successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
