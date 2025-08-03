using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.UserFollow;

namespace Application.Features.UserFollow.Queries
{
    public class GetFollowers : IRequest<ApiResponse>
    {
        public string? Username { get; set; }
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
            if (string.IsNullOrWhiteSpace(request.Username))
                return new ApiResponse
                {
                    Success = false,
                    Message = "Username is required."
                };

            var user = await _userRepo.GetByName(request.Username);
            if (user == null)
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };

            var followerIds = await _followRepo.GetFollowerIdsOfUserAsync(user.id);

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
                if (userDict.TryGetValue(id, out var followerUser))
                {
                    response.Add(new UserFollowResponse
                    {
                        Id = followerUser.id,
                        UserName = followerUser.username,
                        DisplayName = followerUser.displayname,
                        Avatar = followerUser.avata_url
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