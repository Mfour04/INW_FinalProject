using AutoMapper;
using Domain.Entities;
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
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class CreateNovelFollowerHandler : IRequestHandler<CreateNovelFollowerCommand, ApiResponse>
    {
        private readonly IMapper _mapper;
        private readonly INovelFollowRepository _novelFollowRepository;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        
        public CreateNovelFollowerHandler(IMapper mapper, INovelFollowRepository novelFollowRepository, IUserRepository userRepository, INovelRepository novelRepository)
        {
            _mapper = mapper;
            _novelFollowRepository = novelFollowRepository;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(CreateNovelFollowerCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy người dùng này" };

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy novel này" };
            var existingFollow = await _novelFollowRepository.GetByUserAndNovelIdAsync(request.UserId, request.NovelId);
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
                followed_at = TimeHelper.NowTicks
            };

            await _novelFollowRepository.CreateNovelFollowAsync(novelfollower);
            await _novelRepository.IncrementFollowersAsync(request.NovelId);
            var response = _mapper.Map<CreateNovelFollowReponse>(novelfollower);

            return new ApiResponse
            {
                Success = true,
                Message = "Novel Follower create succesfully",
                Data = response
            };
        }
    }
}
