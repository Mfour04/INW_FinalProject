using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.User.Feature
{
    public class VerifyUserCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
    public class VerifyUserHandler : IRequestHandler<VerifyUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IBadgeProgressService _badgeProgressService;

        public VerifyUserHandler(IUserRepository userRepository, IBadgeProgressService badgeProgressService)
        {
            _userRepository = userRepository;
            _badgeProgressService = badgeProgressService;
        }

        public async Task<ApiResponse> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy người dùng." };

            if (user.is_verified)
                return new ApiResponse { Success = false, Message = "Người dùng đã được xác thực." };

            user.is_verified = true;
            user.updated_at = TimeHelper.NowTicks;

            await _userRepository.UpdateUser(user);

            await _badgeProgressService.InitializeUserBadgeProgress(user.id);

            return new ApiResponse { Success = true, Message = "Xác thực email thành công." };

        }
    }
}
