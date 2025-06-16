using AutoMapper;
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

            var response = new List<PostCommentResponse>();

            foreach (var comment in postCommentList)
            {
                var mapped = _mapper.Map<PostCommentResponse>(comment);

                var user = await _userRepo.GetById(comment.user_id);
                if (user != null)
                {
                    mapped.Author = new ForumPostCommentAuthorResponse
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
                Message = "Retrieved forum post comments successfully.",
                Data = response
            };
        }
    }
}