using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Rating.Command
{
    public class CreateRatingCommand : IRequest<ApiResponse>
    {
        public string? NovelId { get; set; }
        public int Score { get; set; }
        public string? Content { get; set; }
    }

    public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUser;

        public CreateRatingCommandHandler(
            IRatingRepository ratingRepository,
            INovelRepository novelRepository,
            ICurrentUserService currentUser)
        {
            _ratingRepository = ratingRepository;
            _novelRepository = novelRepository;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(request);
            if (!validation.IsValid)
                return validation.Response!;

            var rating = new RatingEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = request.NovelId,
                user_id = _currentUser.UserId,
                content = request.Content,
                score = request.Score,
                created_at = TimeHelper.NowTicks
            };

            await _ratingRepository.CreateAsync(rating);

            var avg = await _ratingRepository.GetAverageRatingByNovelIdAsync(request.NovelId);
            var count = await _ratingRepository.GetRatingCountByNovelIdAsync(request.NovelId);
            
            await _novelRepository.UpdateRatingStatsAsync(request.NovelId!, avg, count);

            return new ApiResponse
            {
                Success = true,
                Message = "Đã tạo rating thành công."
            };
        }

        private async Task<(bool IsValid, ApiResponse? Response)> ValidateRequestAsync(CreateRatingCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
                return (false, Fail("Cần phải có ID truyện."));

            if (request.Score < 1 || request.Score > 5)
                return (false, Fail("Điểm đánh giá phải từ 1 đến 5."));

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return (false, Fail("Không tìm thấy truyện."));

            var hasRated = await _ratingRepository.HasUserRatedNovelAsync(_currentUser.UserId, request.NovelId);
            if (hasRated)
                return (false, Fail("Bạn đã đánh giá truyện này rồi."));

            return (true, null);
        }
        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
