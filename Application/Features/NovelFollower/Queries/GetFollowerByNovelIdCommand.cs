using AutoMapper;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Contracts.Response.Follow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.NovelFollower.Queries
{
    public class GetFollowerByNovelIdCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
    }
    public class GetFollowerByNovelIdHanlder : IRequestHandler<GetFollowerByNovelIdCommand, ApiResponse>
    {
        private readonly INovelFollowRepository _novelFollowRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public GetFollowerByNovelIdHanlder(INovelFollowRepository novelFollowRepository, IMapper mapper
            , IUserRepository userRepository)
        {
            _novelFollowRepository = novelFollowRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse> Handle(GetFollowerByNovelIdCommand request, CancellationToken cancellationToken)
        {
            var follows = await _novelFollowRepository.GetFollowersByNovelIdAsync(request.NovelId);
            var result = new GetNovelFollowersResponse
            {
                NovelId = request.NovelId,
                TotalFollowers = follows.Count,
                Followers = new List<NovelFollowerUserInfoResponse>()
            };

            foreach (var follow in follows)
            {
                var user = await _userRepository.GetById(follow.user_id);
                if (user == null)
                    return new ApiResponse { Success = false, Message = "User not found" };
                var mapped = _mapper.Map<NovelFollowerUserInfoResponse>((follow, user));
                result.Followers.Add(mapped);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Followers retrieved successfully.",
                Data = result
            };
        }
    }
}
