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

        public UnlikePostCommandHandler(IForumPostLikeRepository postLikeRepo)
        {
            _postLikeRepo = postLikeRepo;
        }

        public async Task<ApiResponse> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
        {
            var hasLiked = await _postLikeRepo.HasUserLikedPostAsync(request.PostId, request.UserId);

            if (!hasLiked)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User has not liked this post."
                };
            }

            var isSuccess = await _postLikeRepo.UnlikePostAsync(request.PostId, request.UserId);

            if (!isSuccess)
                return new ApiResponse { Success = false, Message = "Failed to unlike the post." };

            return new ApiResponse
            {
                Success = true,
                Message = "Unlike successfully.",
            };
        }
    }
}