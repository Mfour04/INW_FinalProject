using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;
using Shared.Helpers;

namespace Application.Features.Rating.Queries
{
    public sealed class GetNovelRatingsKeyset : IRequest<ApiResponse>
    {
        public string NovelId { get; set; } = default!;
        public int Limit { get; set; } = 5;
        public string? AfterId { get; set; }
    }

    public sealed class GetNovelRatingsKeysetHandler : IRequestHandler<GetNovelRatingsKeyset, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetNovelRatingsKeysetHandler(
            IRatingRepository ratingRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetNovelRatingsKeyset req, CancellationToken ct)
        {
            var limit = Math.Clamp(req.Limit, 1, 50);

            long? afterTicks = null;
            if (!string.IsNullOrWhiteSpace(req.AfterId))
            {
                if (CursorHelper.TryParseShortCursor20(req.AfterId, req.NovelId, out var ticks, out var expired)
                    && !expired)
                {
                    afterTicks = ticks;
                }
            }

            var (items, hasMore) = await _ratingRepository.GetByNovelIdKeysetAsync(
                req.NovelId,
                limit,
                afterTicks,
                ct);

            var responses = _mapper.Map<List<RatingResponse>>(items);

            var userIds = items.Select(x => x.user_id)
                               .Where(id => !string.IsNullOrWhiteSpace(id))
                               .Distinct()
                               .ToList();

            if (userIds.Count > 0)
            {
                var fetchTasks = userIds.Select(id => _userRepository.GetById(id)).ToList();
                var users = await Task.WhenAll(fetchTasks);
                var userMap = users.Where(u => u != null && !string.IsNullOrWhiteSpace(u!.id))
                                   .ToDictionary(u => u!.id, u => u!);

                for (int i = 0; i < items.Count; i++)
                {
                    var e = items[i];
                    var dto = responses[i];

                    if (!string.IsNullOrWhiteSpace(e.user_id) &&
                        userMap.TryGetValue(e.user_id, out var u))
                    {
                        dto.Author = new RatingResponse.UserInfo
                        {
                            Id = u.id,
                            Username = u.username,
                            DisplayName = u.displayname,
                            Avatar = u.avata_url
                        };
                    }
                }
            }

            var last = items.LastOrDefault();
            var nextAfterId = last is null ? null : CursorHelper.MakeShortCursor20(req.NovelId, last.created_at);

            var result = new RatingListKeysetResponse
            {
                Items = responses,
                HasMore = hasMore,
                NextAfterId = nextAfterId
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
