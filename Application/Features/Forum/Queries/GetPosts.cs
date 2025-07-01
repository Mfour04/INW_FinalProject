using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

namespace Application.Features.Forum.Queries
{
    public class GetPosts : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetPostsHanlder : IRequestHandler<GetPosts, ApiResponse>
    {
        private readonly IForumPostRepository _postRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetPostsHanlder(IForumPostRepository postRepo, IUserRepository userRepo, IMapper mapper)
        {
            _postRepo = postRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPosts request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var postList = await _postRepo.GetAllAsync(findCreterias, sortBy);

            if (postList == null || postList.Count == 0)
                return new ApiResponse { Success = false, Message = "No forum posts found." };

            var response = new List<PostResponse>();

            foreach (var post in postList)
            {
                var mapped = _mapper.Map<PostResponse>(post);

                var user = await _userRepo.GetById(post.user_id);
                if (user != null)
                {
                    mapped.Author = new ForumPostAuthorResponse
                    {
                        Id = user.id,
                        Username = user.username,
                        Avatar = user.avata_url
                    };
                }

                response.Add(mapped);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved forum posts successfully.",
                Data = response
            };
        }
    }
}