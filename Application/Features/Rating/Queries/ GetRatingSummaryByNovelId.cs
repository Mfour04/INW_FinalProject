using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Rating.Queries
{
    public class GetRatingSummaryByNovelId : IRequest<ApiResponse>
    {
        public string? NovelId { get; set; }
    }

    public class GetRatingSummaryByNovelIdHandler : IRequestHandler<GetRatingSummaryByNovelId, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;

        public GetRatingSummaryByNovelIdHandler(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<ApiResponse> Handle(GetRatingSummaryByNovelId request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
                return Fail("Novel ID is required.");

            var allRatings = await _ratingRepository.GetByNovelIdAsync(
                    request.NovelId,
                    new FindCreterias { Limit = int.MaxValue, Page = 0 },
                    new List<SortCreterias>()
              );

            if (allRatings.Count == 0)
                return Fail("No ratings found for this novel.");

            var avg = Math.Round(allRatings.Average(r => r.score), 2);

            var scoreCounts = allRatings
                .GroupBy(r => r.score)
                .Select(g => new { Score = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Score)
                .ToDictionary(x => x.Score, x => x.Count);

            var result = new
            {
                ratingCount = allRatings.Count,
                ratingAvg = avg,
                scoreDistribution = scoreCounts
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Rating summary retrieved successfully.",
                Data = result
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
