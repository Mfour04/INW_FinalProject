using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Rating.Queries
{
    public class GetRatingByNovelId: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }
    public class GetRatingByNovelIdHandler : IRequestHandler<GetRatingByNovelId, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        public GetRatingByNovelIdHandler(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }
        public async Task<ApiResponse> Handle(GetRatingByNovelId request, CancellationToken cancellationToken)
        {
            try
            {
                var creterias = new FindCreterias
                {
                    Page = request.Page,
                    Limit = request.Limit
                };

                var (ratings, totalRatings, totalPages) = await _ratingRepository.GetRatingByNovelIdAsync(request.NovelId, creterias);

                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        Ratings = ratings,
                        TotalRatings = totalRatings,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }   
}
