using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;

namespace Application.Features.Comment.Queries
{
    public class GetRepliesComment : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetRepliesCommentHandler : IRequestHandler<GetRepliesComment, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetRepliesCommentHandler(ICommentRepository commentRepo, IUserRepository userRepo, IMapper mapper)
        {
            _commentRepo = commentRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetRepliesComment request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepo.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found."
                };
            }

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var repliesList = await _commentRepo.GetRepliesByCommentIdAsync(request.CommentId, findCreterias, sortBy);

            if (repliesList == null || repliesList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No Comment's reply found."
                };
            }

            var userIds = repliesList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<CommentReplyResponse>();

            foreach (var reply in repliesList)
            {
                var mapped = _mapper.Map<CommentReplyResponse>(reply);

                if (userDict.TryGetValue(reply.user_id, out var user))
                {
                    mapped.Author = new BaseCommentResponse.UserInfo
                    {
                        Id = user.id,
                        UserName = user.username,
                        DisplayName = user.displayname,
                        Avatar = user.avata_url
                    };
                }
                else
                {
                    mapped.Author = new BaseCommentResponse.UserInfo();
                }

                response.Add(mapped);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved reply comments successfully.",
                Data = response
            };
        }
    }
}