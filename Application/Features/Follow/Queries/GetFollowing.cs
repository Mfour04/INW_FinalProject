using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.UserFollow;

namespace Application.Features.Follow.Queries
{
    public class GetFollowing : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetFollowingHandler : IRequestHandler<GetFollowing, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;

        public GetFollowingHandler(IUserFollowRepository followRepo, IUserRepository userRepo)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(GetFollowing request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return new ApiResponse
                {
                    Success = false,
                    Message = "UserId is required."
                };

            var followingIds = await _followRepo.GetFollowingIdsOfUserAsync(request.UserId);

            if (followingIds == null || !followingIds.Any())
                return new ApiResponse
                {
                    Success = false,
                    Message = "This user is not following anyone."
                };

            var users = await _userRepo.GetUsersByIdsAsync(followingIds);
            var userDict = users.ToDictionary(u => u.id, u => u);

            var response = new List<UserFollowResponse>();

            foreach (var id in followingIds)
            {
                if (userDict.TryGetValue(id, out var user))
                {
                    response.Add(new UserFollowResponse
                    {
                        Id = user.id,
                        UserName = user.username,
                        DisplayName = user.displayname,
                        Avatar = user.avata_url
                    });
                }
            }

            return new ApiResponse
            {
                Success = true,
                Data = response
            };
        }
    }
}