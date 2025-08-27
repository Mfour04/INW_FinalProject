using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response.Forum;
using Shared.Contracts.Response;

namespace Application.Features.UserFollow.Queries
{
    public class CheckFollowStatus : IRequest<ApiResponse>
    {
        public string FollowerId { get; set; } = string.Empty;
        public string TargetUserId { get; set; } = string.Empty;
    }

    public class CheckFollowStatusHandler : IRequestHandler<CheckFollowStatus, ApiResponse>
    {
        private readonly IUserFollowRepository _userFollowRepository;

        public CheckFollowStatusHandler(IUserFollowRepository userFollowRepository)
        {
            _userFollowRepository = userFollowRepository;
        }

        public async Task<ApiResponse> Handle(CheckFollowStatus request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if current user is following target user
                var isFollowing = await _userFollowRepository.IsFollowingAsync(request.FollowerId, request.TargetUserId);

                // Check if target user is following current user
                var isFollowedBy = await _userFollowRepository.IsFollowingAsync(request.TargetUserId, request.FollowerId);

                var response = new CheckFollowStatusResponse
                {
                    IsFollowing = isFollowing,
                    IsFollowedBy = isFollowedBy
                };

                return new ApiResponse
                {
                    Success = true,
                    Message = "Follow status retrieved successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Failed to check follow status: {ex.Message}"
                };
            }
        }
    }
}
