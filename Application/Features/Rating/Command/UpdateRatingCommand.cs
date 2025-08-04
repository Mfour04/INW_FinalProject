using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rating.Command
{
    public class UpdateRatingCommand : IRequest<ApiResponse>
    {
        public string RatingId { get; set; }
        public int Score { get; set; }
        public string? RatingContent { get; set; }
    }
    public class UpdateRatingCommandHandler : IRequestHandler<UpdateRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMapper _mapper;

        public UpdateRatingCommandHandler(IRatingRepository ratingRepository, IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(UpdateRatingCommand request, CancellationToken cancellationToken)
        {
            var input = request;
            var rating = await _ratingRepository.GetByIdAsync(input.RatingId);
            if (rating == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy đánh giá."
                };
            }
            rating.score = input.Score;
            rating.rating_content = input.RatingContent;
            rating.updated_at = TimeHelper.NowTicks;
            var updatedRating = await _ratingRepository.UpdateAsync(rating);
            var response = _mapper.Map<UpdateRatingResponse>(updatedRating);
            if (updatedRating == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy đánh giá hoặc gặp lỗi khi cập nhật đánh giá."
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật đánh giá thành công.",
                Data = response
            };
        }
    }
}
