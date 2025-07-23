using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Queries
{
    public class CheckSlug : IRequest<ApiResponse>
    {
        public string Slug { get; set; }
    }

    public class CheckSlugHandler : IRequestHandler<CheckSlug, ApiResponse>
    {
        private readonly INovelRepository _novelRepo;

        public CheckSlugHandler(INovelRepository novelRepo)
        {
            _novelRepo = novelRepo;
        }

        public async Task<ApiResponse> Handle(CheckSlug request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Slug is required"
                };
            }

            var exists = await _novelRepo.IsSlugExistsAsync(request.Slug);

            return new ApiResponse
            {
                Success = true,
                Message = "Slug check completed",
                Data = new { exists }
            };
        }
    }
}