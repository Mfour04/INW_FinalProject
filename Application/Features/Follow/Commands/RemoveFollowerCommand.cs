using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Follow.Commands
{
    public class RemoveFollowerCommand : IRequest<ApiResponse>
    {
        public string? CurrentUserId { get; set; }
        public string? FollowerToRemoveId { get; set; }
    }

    public class RemoveFollowerCommandHandler : IRequestHandler<RemoveFollowerCommand, ApiResponse>
    {
        private readonly IUserFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;

        public RemoveFollowerCommandHandler(IUserFollowRepository followRepo, IUserRepository userRepo)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(RemoveFollowerCommand request, CancellationToken cancellationToken)
        {
            var success = await _followRepo.UnfollowAsync(request.FollowerToRemoveId, request.CurrentUserId);
            if (!success)
                return new ApiResponse { Success = false, Message = "The follow does not exist to unfollow." };

            await _userRepo.IncrementFollowerCountAsync(request.CurrentUserId, -1);
            await _userRepo.IncrementFollowingCountAsync(request.FollowerToRemoveId, -1);

            return new ApiResponse
            {
                Success = true,
                Message = "Follower removed successfully."
            };
        }
    }
}