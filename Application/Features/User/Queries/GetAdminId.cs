using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;


namespace Application.Features.User.Queries
{
    public class GetAdminId : IRequest<ApiResponse>
    {
    }
    public class GetAdminIdHandler : IRequestHandler<GetAdminId, ApiResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetAdminIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetAdminId request, CancellationToken cancellationToken)
        {
            var adminUser = await _userRepository.GetFirstUserByRoleAsync(Role.Admin);

            if (adminUser == null)
            {
                return new ApiResponse { Success = false, Message = "Not found admin" };
            }

            return new ApiResponse { Success = true, Message = " found admin", Data = adminUser};
        }
    }
}
