using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;
using static Shared.Contracts.Response.Forum.PostCommentResponse;

namespace Application.Features.Forum.Queries
{
    public class GetPostComments : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string? PostId { get; set; }
    }

    public class GetPostCommentsHandler : IRequestHandler<GetPostComments, ApiResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IMapper _mapper;

        public GetPostCommentsHandler(
            IForumCommentRepository postCommentRepo,
            IUserRepository userRepo,
            IMapper mapper)
        {
            _postCommentRepo = postCommentRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPostComments request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var postCommentList = await _postCommentRepo.GetAllByPostIdAsync(request.PostId, findCreterias, sortBy);

            if (postCommentList == null || postCommentList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No forum post comments found."
                };
            }

            var userIds = postCommentList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<PostCommentResponse>();

            foreach (var comment in postCommentList)
            {
                var mapped = _mapper.Map<PostCommentResponse>(comment);

                if (userDict.TryGetValue(comment.user_id, out var user))
                {
                    mapped.Author = new PostCommentAuthorResponse
                    {
                        Id = user.id,
                        Username = user.username,
                        Avatar = user.avata_url
                    };
                }
                else
                {
                    mapped.Author = new PostCommentAuthorResponse(); 
                }

                response.Add(mapped);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved forum post comments successfully.",
                Data = response
            };
        }
    }
}