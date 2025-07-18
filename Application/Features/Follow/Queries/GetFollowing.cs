using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Follow.Queries
{
    public class GetFollowing : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetFollowingHandler : IRequestHandler<GetFollowing, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;

        public GetFollowingHandler(IUserFollowRepository followRepo)
        {
            _followRepo = followRepo;
        }

        public async Task<ApiResponse> Handle(GetFollowing request, CancellationToken cancellationToken)
        {
            var listFollowing = await _followRepo.GetFollowingAsync(request.UserId);

            if (listFollowing == null || listFollowing.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No following found for the user."
                };
            }

            return new ApiResponse
            {
                Success = true,
                Data = listFollowing
            };
        }
    }
}