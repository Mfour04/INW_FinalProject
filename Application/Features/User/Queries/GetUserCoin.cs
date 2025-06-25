using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.User.Queries
{
    public class GetUserCoin : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetUserCoinHandler : IRequestHandler<GetUserCoin, ApiResponse>
    {
        private readonly IUserRepository _userRepo;

        public GetUserCoinHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(GetUserCoin request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "User not found." };

            return new ApiResponse
            {
                Success = true,
                Data = new { Coin = user.coin }
            };
        }
    }
}