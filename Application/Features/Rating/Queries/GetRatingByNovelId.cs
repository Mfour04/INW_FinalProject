using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;
using Shared.Helpers;

namespace Application.Features.Rating.Queries
{
    public class GetRatingByNovelId : IRequest<ApiResponse>
    {
        public string? NovelId { get; set; }
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetRatingByNovelIdHandler : IRequestHandler<GetRatingByNovelId, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMapper _mapper;

        public GetRatingByNovelIdHandler(IRatingRepository ratingRepository, IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetRatingByNovelId request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Novel ID is required."
                };
            }

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var ratings = await _ratingRepository.GetByNovelIdAsync(request.NovelId, findCreterias, sortBy);
            var count = ratings.Count;
            var avg = count > 0 ? Math.Round(ratings.Average(r => r.score), 2) : 0;

            if (count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No ratings found for this novel."
                };
            }

            var scoreCounts = ratings
                .GroupBy(r => r.score)
                .Select(g => new { Score = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Score)
                .ToDictionary(x => x.Score, x => x.Count);

            var ratingResponses = _mapper.Map<List<RatingResponse>>(ratings);

            var result = new
            {
                ratingCount = count,
                ratingAvg = avg,
                scoreDistribution = scoreCounts,
                ratings = ratingResponses
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Ratings retrieved successfully.",
                Data = result
            };
        }
    }
}
