using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.UserFollow.Commands
{
    public class UnfollowUserCommand : IRequest<ApiResponse>
    {
        public string? FollowerId { get; set; }
        public string? FollowingId { get; set; }
    }

    public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;

        public UnfollowUserCommandHandler(IUserFollowRepository followRepo, IUserRepository userRepo)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {
            var success = await _followRepo.UnfollowAsync(request.FollowerId, request.FollowingId);
            if (!success)
                return new ApiResponse { Success = false, Message = "The follow does not exist to unfollow." };

            await _userRepo.IncrementFollowerCountAsync(request.FollowingId, -1);
            await _userRepo.IncrementFollowingCountAsync(request.FollowerId, -1);

            return new ApiResponse
            {
                Success = true,
                Message = "Unfollowed successfully.",
            };
        }
    }
}