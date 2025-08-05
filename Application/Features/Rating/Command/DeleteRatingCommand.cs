using Application.Services.Interfaces;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Rating.Command
{
    public class DeleteRatingCommand : IRequest<ApiResponse>
    {
        public string? RatingId { get; set; }
    }

    public class DeleteRatingCommandHandler : IRequestHandler<DeleteRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUser;

        public DeleteRatingCommandHandler(
            IRatingRepository ratingRepository,
            INovelRepository novelRepository,
            ICurrentUserService currentUser)
        {
            _ratingRepository = ratingRepository;
            _novelRepository = novelRepository;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
        {
            var rating = await _ratingRepository.GetByIdAsync(request.RatingId);
            if (rating == null)
                return Fail("Rating not found or already deleted.");

            if (rating.user_id != _currentUser.UserId)
                return Fail("You are not authorized to delete this rating.");

            var deleted = await _ratingRepository.DeleteAsync(request.RatingId);
            if (!deleted)
                return Fail("Failed to delete rating.");

            var avg = await _ratingRepository.GetAverageRatingByNovelIdAsync(rating.novel_id);
            var count = await _ratingRepository.GetRatingCountByNovelIdAsync(rating.novel_id);
            
            await _novelRepository.UpdateRatingStatsAsync(rating.novel_id, avg, count);

            return new ApiResponse
            {
                Success = true,
                Message = "Rating deleted successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
