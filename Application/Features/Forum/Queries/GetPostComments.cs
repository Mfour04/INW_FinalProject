using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

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
            if (string.IsNullOrEmpty(request.PostId))
                return new ApiResponse { Success = false, Message = "PostId is required." };

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var commentList = await _postCommentRepo.GetRootCommentsByPostIdAsync(request.PostId, findCreterias, sortBy);

            if (commentList == null || commentList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "No Post's comments found.",
                };
            }

            var userIds = commentList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<PostRootCommentResponse>();

            foreach (var comment in commentList)
            {
                var mapped = _mapper.Map<PostRootCommentResponse>(comment);

                if (userDict.TryGetValue(comment.user_id, out var user))
                {
                    mapped.Author = new BasePostCommentResponse.PostCommentAuthorResponse
                    {
                        Id = user.id,
                        Username = user.username,
                        DisplayName = user.displayname,
                        Avatar = user.avata_url
                    };
                }
                response.Add(mapped);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved comments successfully.",
                Data = response
            };
        }
    }
}