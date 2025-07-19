using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Follow;

namespace Application.Features.NovelFollower.Queries
{
    public class GetFollowerByUserId : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
    }
    public class GetFollowerByUserIdHandler : IRequestHandler<GetFollowerByUserId, ApiResponse>
    {
        private readonly INovelFollowRepository _followerRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public GetFollowerByUserIdHandler(INovelFollowRepository followerRepository, INovelRepository novelRepository
            , IUserRepository userRepository, IMapper mapper)
        {
            _followerRepository = followerRepository;
            _novelRepository = novelRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(GetFollowerByUserId request, CancellationToken cancellationToken)
        {
            var follows = await _followerRepository.GetFollowedNovelsByUserIdAsync(request.UserId);

            var result = new GetUserFollowedNovelsResponse
            {
                UserId = request.UserId,
                TotalFollowing = follows.Count,
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
                result.FollowedNovels.Add(mapped);

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
