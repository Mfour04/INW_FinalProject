using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Rating.Command
{
    public class UpdateRatingCommand : IRequest<ApiResponse>
    {
        public string? RatingId { get; set; }
        public int Score { get; set; }
        public string? Content { get; set; }
    }
 
    public class UpdateRatingCommandHandler : IRequestHandler<UpdateRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUser;

        public UpdateRatingCommandHandler(
            IRatingRepository ratingRepository,
            INovelRepository novelRepository,
            ICurrentUserService currentUser)
        {
            _ratingRepository = ratingRepository;
            _novelRepository = novelRepository;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(UpdateRatingCommand request, CancellationToken cancellationToken)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.RatingId);
            if (rating == null)
                return Fail("Không tìm thấy đánh giá.");

            if (rating.user_id != _currentUser.UserId)
                return Fail("Bạn không có quyền cập nhật đánh giá này.");

            RatingEntity updated = new()
            {
                score = request.Score,
                content = request.Content,
            };

            var success = await _ratingRepository.UpdateAsync(request.RatingId, updated);
            if (!success)
                return Fail("Cập nhật đánh giá thất bại.");

            var avg = await _ratingRepository.GetAverageRatingByNovelIdAsync(rating.novel_id);
            var count = await _ratingRepository.GetRatingCountByNovelIdAsync(rating.novel_id);

            await _novelRepository.UpdateRatingStatsAsync(rating.novel_id, avg, count);

            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật đánh giá thành công."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
