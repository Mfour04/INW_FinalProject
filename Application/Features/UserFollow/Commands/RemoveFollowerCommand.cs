using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.UserFollow.Commands
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
            if (string.IsNullOrWhiteSpace(request.CurrentUserId) || string.IsNullOrWhiteSpace(request.FollowerToRemoveId))
            {
                return new ApiResponse { Success = false, Message = "Dữ liệu đầu vào không hợp lệ." };
            }

            bool isFollowing = await _followRepo.IsFollowingAsync(request.FollowerToRemoveId, request.CurrentUserId);
            if (!isFollowing)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Người dùng này không đang theo dõi bạn."
                };
            }

            var success = await _followRepo.UnfollowAsync(request.FollowerToRemoveId, request.CurrentUserId);
            if (!success)
            {
                return new ApiResponse { Success = false, Message = "Xóa người theo dõi thất bại." };
            }

            await _userRepo.IncrementFollowerCountAsync(request.CurrentUserId, -1);
            await _userRepo.IncrementFollowingCountAsync(request.FollowerToRemoveId, -1);

            return new ApiResponse
            {
                Success = true,
                Message = "Đã xóa người theo dõi thành công."
            };
        }
    }
}