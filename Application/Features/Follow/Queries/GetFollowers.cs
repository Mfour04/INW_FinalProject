using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.UserFollow;

namespace Application.Features.Follow.Queries
{
    public class GetFollowers : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetFollowersHandler : IRequestHandler<GetFollowers, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;

        public GetFollowersHandler(IUserFollowRepository followRepo, IUserRepository userRepo)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(GetFollowers request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return new ApiResponse
                {
                    Success = false,
                    Message = "UserId is required."
                };

            var followerIds = await _followRepo.GetFollowerIdsOfUserAsync(request.UserId);

            if (followerIds == null || !followerIds.Any())
                return new ApiResponse
                {
                    Success = false,
                    Message = "This user has no followers yet."
                };

            var users = await _userRepo.GetUsersByIdsAsync(followerIds);
            var userDict = users.ToDictionary(u => u.id, u => u);

            var response = new List<UserFollowResponse>();

            foreach (var id in followerIds)
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