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
                return Fail("Novel ID is required.");

            FindCreterias findCriterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var getRatingsTask = _ratingRepository.GetByNovelIdAsync(request.NovelId, findCriterias, sortBy);
            var getCountTask = _ratingRepository.GetRatingCountByNovelIdAsync(request.NovelId);

            await Task.WhenAll(getRatingsTask, getCountTask);

            var ratings = getRatingsTask.Result;
            var totalCount = getCountTask.Result;

            if (totalCount == 0)
                return Fail("No ratings found for this novel.");

            var totalPage = (int)Math.Ceiling(totalCount / (double)request.Limit);

            var ratingResponses = _mapper.Map<List<RatingResponse>>(ratings);

            var result = new
            {
                totalCount,
                totalPage,
                ratings = ratingResponses
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Ratings retrieved successfully.",
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
