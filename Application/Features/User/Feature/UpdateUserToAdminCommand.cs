using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.User.Feature
{
    public class UpdateUserToAdminCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }

    public class UpdateUserToAdminHandler : IRequestHandler<UpdateUserToAdminCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        public UpdateUserToAdminHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<ApiResponse> Handle(UpdateUserToAdminCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "User Not Found" };

            var success = await _userRepository.UpdateUserRoleToAdminAsync(user.id);
            if (!success)
                return Fail("Failed to update the badge.");

            return new ApiResponse
            {
                Success = true,
                Message = "Update to admin successfully"
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
