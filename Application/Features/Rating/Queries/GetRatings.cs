using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rating.Queries
{
    public class GetRatings : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class GetRatingsHandler : IRequestHandler<GetRatings, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;

        public GetRatingsHandler(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<ApiResponse> Handle(GetRatings request, CancellationToken cancellationToken)
        {
            List<RatingEntity> ratings;

            if (!string.IsNullOrEmpty(request.NovelId))
            {
                ratings = await _ratingRepository.GetByNovelIdAsync(request.NovelId);
            }
            else
            {
                ratings = await _ratingRepository.GetAllAsync();
            }
            if (ratings == null || !ratings.Any())
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không có đánh giá nào cho truyện này."
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Lấy đánh giá thành công.",
                Data = ratings
            };
        }
    }
}
