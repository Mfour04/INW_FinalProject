using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.User.Feature
{
    public class UpdateLockvsUnLockUserCommand: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public bool isBanned { get; set; }
    }
    public class UpdateLockvsUnLockUserHandler : IRequestHandler<UpdateLockvsUnLockUserCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        public UpdateLockvsUnLockUserHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse> Handle(UpdateLockvsUnLockUserCommand request, CancellationToken cancellationToken)
        {
            var adminId = _currentUserService.UserId;
            var roles = _currentUserService.Role;

            if (string.IsNullOrEmpty(adminId) || roles == null || !roles.Contains("Admin"))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Forbidden: Only admins can perform this action."
                };
            }

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            await _userRepository.UpdateLockvsUnLockUser(request.UserId, request.isBanned);

            return new ApiResponse
            {
                Success = true,
                Message = request.isBanned
                    ? "User has been banned successfully."
                    : "User has been unbanned successfully."
            };
        }
    }
}
