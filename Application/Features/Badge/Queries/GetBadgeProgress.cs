using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Queries
{
    public class GetBadgeProgress : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
    }

    public class GetBadgeProgressHandler : IRequestHandler<GetBadgeProgress, ApiResponse>
    {
        private readonly IBadgeProgressRepository _progressRepo;

        public GetBadgeProgressHandler(IBadgeProgressRepository progressRepo)
        {
            _progressRepo = progressRepo;
        }

        public async Task<ApiResponse> Handle(GetBadgeProgress request, CancellationToken cancellationToken)
        {
            var progresses = await _progressRepo.GetByUserIdAsync(request.UserId);

            if (progresses == null || progresses.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No badge progress found for the user."
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