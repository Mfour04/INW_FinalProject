using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;

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
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public GetRatingByNovelIdHandler(IRatingRepository ratingRepository, IMapper mapper, IUserRepository userRepository)
        {
            _ratingRepository = ratingRepository;
            _mapper = mapper;
            _userRepository = userRepository;
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
                var userIds = ratings.Select(r => r.user_id).Distinct().ToList();
                var users = await _userRepository.GetUsersByIdsAsync(userIds);
                var userDict = users.ToDictionary(u => u.id, u => u.displayname);

                // Map sang RatingResponse
                var response = ratings.Select(rating =>
                {
                    var mapped = _mapper.Map<RatingResponse>(rating);
                    mapped.DisplayName = userDict.TryGetValue(rating.user_id, out var name) ? name : "Unknown";
                    return mapped;
                }).ToList();

                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        Ratings = response,
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
