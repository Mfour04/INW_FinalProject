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
        private readonly INovelRepository _novelRepository;
        public GetFollowerByNovelIdHanlder(INovelFollowRepository novelFollowRepository, IMapper mapper
            , IUserRepository userRepository, INovelRepository novelRepository)
        {
            _novelFollowRepository = novelFollowRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(GetFollowerByNovelIdCommand request, CancellationToken cancellationToken)
        {
            var follows = await _novelFollowRepository.GetFollowersByNovelIdAsync(request.NovelId);
            if (follows == null || follows.Count == 0)
                return new ApiResponse { Success = false, Message = "No followers found for this novel" };
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Novel not found" };
            var author = await _userRepository.GetById(novel.author_id);
            if (author == null)
                return new ApiResponse { Success = false, Message = "Author not found" };

            var result = new GetNovelFollowersResponse
            {
                NovelId = request.NovelId,
                Title = novel.title,
                NovelImage = novel.novel_image,
                NovelBanner = novel.novel_banner,
                AuthorId = novel.author_id,
                AuthorName = author.displayname,
                Price = novel.price,
                TotalChapters = novel.total_chapters,
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
