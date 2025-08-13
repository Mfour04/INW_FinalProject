using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Queries
{
    public class GetCompletedBadgeProgress : IRequest<ApiResponse>
    {
        public string Username { get; set; }
    }

    public class GetCompletedBadgeProgressHandler : IRequestHandler<GetCompletedBadgeProgress, ApiResponse>
    {
        private readonly IBadgeProgressRepository _progressRepo;
        private readonly IUserRepository _userRepo;

        public GetCompletedBadgeProgressHandler(IBadgeProgressRepository progressRepo, IUserRepository userRepo)
        {
            _progressRepo = progressRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(GetCompletedBadgeProgress request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return new ApiResponse
                {
                    Success = false,
                    Message = "Username is required."
                };

            var user = await _userRepo.GetByName(request.Username);
            if (user == null)
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };

            var progresses = await _progressRepo.GetCompletedByUserIdAsync(user.id);

            if (progresses == null || progresses.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No completed badges found."
                };
            }

            return new ApiResponse
            {
                Success = true,
                Data = progresses
            };
        }
    }
}