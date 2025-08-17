using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;

namespace Application.Features.Rating.Queries
{
    public sealed class GetNovelRatingsKeyset : IRequest<ApiResponse>
    {
        public string NovelId { get; set; } = default!;
        public int Limit { get; set; } = 5;
        public long? AfterCreatedAtTicks { get; set; }
        public string? AfterId { get; set; }
    }

    public sealed class GetNovelRatingsKeysetHandler : IRequestHandler<GetNovelRatingsKeyset, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMapper _mapper;

        public GetNovelRatingsKeysetHandler(IRatingRepository ratingRepository, IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetNovelRatingsKeyset req, CancellationToken ct)
        {
            var (items, hasMore) = await _ratingRepository.GetByNovelIdKeysetAsync(
                req.NovelId,
                Math.Clamp(req.Limit, 1, 50),
                req.AfterCreatedAtTicks,
                req.AfterId,
                ct);

            var responses = _mapper.Map<List<RatingResponse>>(items);

            var last = items.LastOrDefault();
            var result = new RatingListKeysetResponse
            {
                Items = responses,
                HasMore = hasMore,
                NextAfterId = last?.id,
                NextAfterCreatedAt = last?.created_at
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Rating summary retrieved successfully.",
                Data = result
            };
        }
    }
}