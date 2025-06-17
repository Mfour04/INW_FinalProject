using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Badge.Queries
{
    public class GetCompletedBadgeProgress : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }

    public class GetCompletedBadgeProgressHandler : IRequestHandler<GetCompletedBadgeProgress, ApiResponse>
    {
        private readonly IBadgeProgressRepository _progressRepo;

        public GetCompletedBadgeProgressHandler(IBadgeProgressRepository progressRepo)
        {
            _progressRepo = progressRepo;
        }

        public async Task<ApiResponse> Handle(GetCompletedBadgeProgress request, CancellationToken cancellationToken)
        {
            var progresses = await _progressRepo.GetCompletedByUserIdAsync(request.UserId);

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