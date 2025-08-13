using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;


namespace Application.Features.User.Queries
{
    public class GetAdminId : IRequest<ApiResponse>
    {
    }
    public class GetAdminIdHandler : IRequestHandler<GetAdminId, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAdminIdHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetAdminId request, CancellationToken cancellationToken)
        {
            var adminUser = await _userRepository.GetFirstUserByRoleAsync(Role.Admin);

            if (adminUser == null)
            {
                return new ApiResponse { Success = false, Message = "Not found admin" };
            }
            var response = _mapper.Map<UserResponse>(adminUser);
            return new ApiResponse { Success = true, Message = " found admin", Data = response};
        }
    }
}
