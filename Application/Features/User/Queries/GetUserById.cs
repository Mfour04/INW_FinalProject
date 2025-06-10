using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.User.Queries
{
    public class GetUserById : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }

    public class GetUserByIdHanlder : IRequestHandler<GetUserById, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        public GetUserByIdHanlder(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApiResponse> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                    return new ApiResponse { Success = false, Message = "User not found" };

                return new ApiResponse { Success = true, Data = user };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
