using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Follow;
using Shared.Contracts.Response.Novel;
using Shared.Contracts.Response.Tag;

namespace Application.Features.NovelFollower.Queries
{
    public class GetFollowerByUserId : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }
    public class GetFollowerByUserIdHandler : IRequestHandler<GetFollowerByUserId, ApiResponse>
    {
        private readonly INovelFollowRepository _followerRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;
        public GetFollowerByUserIdHandler(INovelFollowRepository followerRepository, INovelRepository novelRepository
            , IUserRepository userRepository, IMapper mapper, ITagRepository tagRepository)
        {
            _followerRepository = followerRepository;
            _novelRepository = novelRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }
        public async Task<ApiResponse> Handle(GetFollowerByUserId request, CancellationToken cancellationToken)
        {
            var findCriteria = new FindCreterias
            {
                Page = request.Page,
                Limit = request.Limit
            };

            var (follows, totalCount) = await _followerRepository.GetFollowedNovelsByUserIdAsync(request.UserId, findCriteria);
            if (follows == null || follows.Count == 0)
                return new ApiResponse { Success = false, Message = "No followed novels found for this user" };

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
                return new ApiResponse { Success = false, Message = "User not found" };

            var result = new GetUserFollowedNovelsResponse
            {
                UserId = request.UserId,
                UserName = user.username,
                DisplayName = user.displayname,
                AvatarUrl = user.avata_url,
                TotalFollowing = totalCount,
                FollowedNovels = new()
            };

            foreach (var follow in follows)
            {
                var novel = await _novelRepository.GetByNovelIdAsync(follow.novel_id);
                if (novel == null) 
                    return new ApiResponse { Success = false, Message = "Novel not found"};

                var author = await _userRepository.GetById(novel.author_id);
                if (author == null)
                    return new ApiResponse { Success = false, Message = "Author not found" };

                var mapped = _mapper.Map<UserFollowingNovelInfoResponse>((follow, novel, author));
                if (novel.tags != null && novel.tags.Any())
                {
                    var tags = await _tagRepository.GetTagsByIdsAsync(novel.tags);
                    mapped.Tags = tags.Select(tag => new TagListResponse
                    {
                        TagId = tag.id,
                        Name = tag.name
                    }).ToList();
                }
                result.FollowedNovels.Add(mapped);

            }

            int totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

            return new ApiResponse
            {
                Success = true,
                Message = "Followers retrieved successfully.",
                Data = new 
                {
                    NovelFollows = result,
                    TotalNovelFollows = totalCount,
                    TotalPages = totalPages
                }
            };

        }
    }
}
