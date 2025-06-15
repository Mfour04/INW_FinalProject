using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string Content { get; set; }
        public List<string>? ImgUrls { get; set; } = new();
    }

    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;

        public CreatePostCommandHandler(IForumPostRepository postRepo)
        {
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new ApiResponse { Success = false, Message = "Content cannot be empty." };

            ForumPostEntity newPost = new()
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                content = request.Content,
                img_urls = request.ImgUrls ?? new List<string>(),
                like_count = 0,
                comment_count = 0,
                created_at = DateTime.Now.Ticks
            };

            await _postRepo.CreateAsync(newPost);

            return new ApiResponse
            {
                Success = true,
                Message = "Post created successfully.",
                Data = newPost
            };
        }
    }
}