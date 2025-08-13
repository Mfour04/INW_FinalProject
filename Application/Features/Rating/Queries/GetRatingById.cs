using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;

namespace Application.Features.Rating.Queries
{
    public class GetRatingById : IRequest<ApiResponse>
    {
        public string RatingId { get; set; }
    }

    public class GetRatingByIdHandler : IRequestHandler<GetRatingById, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetRatingByIdHandler(IRatingRepository ratingRepository, IUserRepository userRepository, IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetRatingById request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RatingId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Rating ID is required."
                };
            }

            var rating = await _ratingRepository.GetByIdAsync(request.RatingId);
            if (rating == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Rating not found."
                };
            }

            var ratingResponse = _mapper.Map<RatingResponse>(rating);

            if (!string.IsNullOrEmpty(rating.user_id))
            {
                var user = await _userRepository.GetById(rating.user_id);
                if (user != null)
                {
                    ratingResponse.Author = new RatingResponse.UserInfo
                    {
                        Id = user.id,
                        Username = user.username,
                        DisplayName = user.displayname,
                        Avatar = user.avata_url
                    };
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Rating retrieved successfully.",
                Data = ratingResponse
            };
        }
    }
}
