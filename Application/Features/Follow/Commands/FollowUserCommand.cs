using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Follow.Commands
{
    public class FollowUserCommand : IRequest<ApiResponse>
    {
        public string? FollowerId { get; set; }
        public string? FollowingId { get; set; }
    }

    public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;

        public FollowUserCommandHandler(IUserFollowRepository followRepo, IUserRepository userRepo)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(FollowUserCommand request, CancellationToken cancellationToken)
        {
            if (await _followRepo.IsFollowingAsync(request.FollowerId, request.FollowingId))
                return new ApiResponse { Success = false, Message = "Already followed." };

            UserFollowEntity follow = new()
            {
                id = SystemHelper.RandomId(),
                follower_id = request.FollowerId,
                following_id = request.FollowingId,
                followed_at = TimeHelper.NowTicks
            };

            await _followRepo.FollowAsync(follow);
            await _userRepo.IncrementFollowerCountAsync(request.FollowingId, 1);
            await _userRepo.IncrementFollowingCountAsync(request.FollowerId, 1);

            return new ApiResponse
            {
                Success = true,
                Message = "Followed successfully.",
            };
        }
    }
}