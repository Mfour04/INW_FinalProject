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
    public class GetRatingById : IRequest<ApiResponse>
    {
        public string RatingId { get; set; }
    }

    public class GetRatingByIdHandler : IRequestHandler<GetRatingById, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;

        public GetRatingByIdHandler(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<ApiResponse> Handle(GetRatingById request, CancellationToken cancellationToken)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.RatingId);
            if (rating == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Rating not found"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Rating retrieved successfully",
                Data = rating
            };
        }
    }
}
