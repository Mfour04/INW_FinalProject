using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

namespace Application.Features.Forum.Queries
{
    public class GetPostCommentReplies : IRequest<ApiResponse>
    {
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
        public string? ParentId { get; set; }
    }

    public class GetPostCommentRepliesHandler : IRequestHandler<GetPostCommentReplies, ApiResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IMapper _mapper;

        public GetPostCommentRepliesHandler(
            IForumCommentRepository postCommentRepo,
            IUserRepository userRepo,
            IMapper mapper)
        {
            _postCommentRepo = postCommentRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPostCommentReplies request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.ParentId))
                return new ApiResponse { Success = false, Message = "ParentId is required." };

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var commentList = await _postCommentRepo.GetRepliesByCommentIdAsync(request.ParentId, findCreterias, sortBy);

            if (commentList == null || commentList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "No Comment's reply found.",
                };
            }

            var userIds = commentList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<PostReplyCommentResponse>();

            foreach (var comment in commentList)
            {
                var mapped = _mapper.Map<PostReplyCommentResponse>(comment);

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
                Message = "Retrieved comment's replies successfully.",
                Data = response
            };
        }
    }
}