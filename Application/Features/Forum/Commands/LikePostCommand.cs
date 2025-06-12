using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class LikePostCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
    }

    public class LikePostCommandHandler : IRequestHandler<LikePostCommand, ApiResponse>
    {
        // private readonly IForumPostRepository _postRepo;
        private readonly IForumPostLikeRepository _postLikeRepo;

        public LikePostCommandHandler(IForumPostLikeRepository postLikeRepo)
        {
            _postLikeRepo = postLikeRepo;
        }

        public async Task<ApiResponse> Handle(LikePostCommand request, CancellationToken cancellationToken)
        {
            var hasLiked = await _postLikeRepo.HasUserLikedPostAsync(request.PostId, request.UserId);
            if (hasLiked)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User has already liked this post."
                };
            }
            
            ForumPostLikeEntity like = new()
            {
                id = SystemHelper.RandomId(),
                post_id = request.PostId,
                user_id = request.UserId,
                like_at = DateTime.Now.Ticks
            };

            await _postLikeRepo.LikePostAsync(like);

            return new ApiResponse
            {
                Success = true,
                Message = "Like successfully.",
                Data = like
            };
        }
    }
}