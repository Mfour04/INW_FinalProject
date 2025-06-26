using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Rating;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.Rating.Command
{
    public class CreateRatingCommand : IRequest<ApiResponse>
    {
        [JsonPropertyName("rating")]
        public CreateRatingResponse CreateRatingResponse { get; set; }
    }
    public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, ApiResponse>
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public CreateRatingCommandHandler(IRatingRepository ratingRepository, IUserRepository userRepository, INovelRepository novelRepository, IMapper mapper)
        {
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
        {
            var existingRating = await _ratingRepository.GetByUserAndNovelAsync(request.CreateRatingResponse.UserId, request.CreateRatingResponse.NovelId);
            if (existingRating != null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Bạn đã đánh giá truyện này rồi."
                };
            }
            var novel = await _novelRepository.GetByNovelIdAsync(request.CreateRatingResponse.NovelId);
            if (novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Không tìm thấy truyện."
                };
            }

            var createdRating = new RatingEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                user_id = request.CreateRatingResponse.UserId,
                score = request.CreateRatingResponse.Score
            };

            await _ratingRepository.CreateAsync(createdRating);
            var response = _mapper.Map<RatingResponse>(createdRating);
            return new ApiResponse
            {
                Success = true,
                Message = "Đánh giá thành công.",
                Data = response
            };
        }
    }
}
