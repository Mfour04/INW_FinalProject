using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Follow.Queries
{
    public class GetFollowers : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetFollowersHandler : IRequestHandler<GetFollowers, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;

        public GetFollowersHandler(IUserFollowRepository followRepo)
        {
            _followRepo = followRepo;
        }

        public async Task<ApiResponse> Handle(GetFollowers request, CancellationToken cancellationToken)
        {
            var listFollowers = await _followRepo.GetFollowersAsync(request.UserId);
            
            if (listFollowers == null || listFollowers.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No follower found for the user."
                };
            }
            
            return new ApiResponse
            {
                Success = true,
                Data = listFollowers
            };
        }
    }
}