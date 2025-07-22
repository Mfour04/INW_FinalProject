using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;

namespace Application.Features.Novel.Queries
{
    public class GetNovelComments : IRequest<ApiResponse>
    {
        public string? NovelId { get; set; }
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetNovelCommentsHandler : IRequestHandler<GetNovelComments, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetNovelCommentsHandler(ICommentRepository commentRepo, INovelRepository novelRepo, IUserRepository userRepo, IMapper mapper)
        {
            _commentRepo = commentRepo;
            _novelRepo = novelRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetNovelComments request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepo.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Novel not found."
                };
            }

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var novelCommentList = await _commentRepo.GetCommentsByNovelIdAsync(request.NovelId, findCreterias, sortBy);

            if (novelCommentList == null || novelCommentList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No Novel's comment found."
                };
            }

            var commentIds = novelCommentList.Select(c => c.id).ToList();
            var replyCountMap = await _commentRepo.CountRepliesPerCommentAsync(commentIds);

            var userIds = novelCommentList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<CommentResponse>();

            foreach (var comment in novelCommentList)
            {
                var mapped = _mapper.Map<CommentResponse>(comment);

                mapped.ReplyCount = replyCountMap.TryGetValue(comment.id, out var count) ? count : 0;

                if (userDict.TryGetValue(comment.user_id, out var user))
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
                Message = "Retrieved novel comments successfully.",
                Data = response
            };
        }
    }
}