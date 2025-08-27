using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Follow;
using Shared.Contracts.Response.NovelFollow;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.NovelFollower.Commands
{
    public class CreateNovelFollowerCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }

    public class CreateNovelFollowerHandler : IRequestHandler<CreateNovelFollowerCommand, ApiResponse>
    {
        private readonly IMapper _mapper;
        private readonly INovelFollowRepository _novelFollowRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateNovelFollowerHandler(IMapper mapper, INovelFollowRepository novelFollowRepository
            , IUserRepository userRepository, INovelRepository novelRepository
            , ICurrentUserService currentUserService)
        {
            _mapper = mapper;
            _novelFollowRepository = novelFollowRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse> Handle(CreateNovelFollowerCommand request, CancellationToken cancellationToken)
        {

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new ApiResponse { Success = false, Message = "Unauthorized" };

            var user = await _userRepository.GetById(userId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy người dùng này" };

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy novel này" };
            var existingFollow = await _novelFollowRepository.GetByUserAndNovelIdAsync(user.id, request.NovelId);
            if (existingFollow != null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User đã follow novel này rồi"
                };
            }
            var novelfollower = new NovelFollowerEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                user_id = user.id,
                username = user.displayname,
                is_notification = true, //Default
                reading_status = NovelFollowReadingStatus.Reading, // Default status
                followed_at = TimeHelper.NowTicks
            };

            await _novelFollowRepository.CreateNovelFollowAsync(novelfollower);
            await _novelRepository.IncrementFollowersAsync(request.NovelId);
            var response = _mapper.Map<CreateNovelFollowReponse>((novelfollower, user));

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Follower đã được tạo thành công",
                Data = response
            };
        }
    }
}
