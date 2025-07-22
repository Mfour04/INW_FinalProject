using AutoMapper;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;

namespace Application.Features.Chapter.Queries
{
    public class GetChapterComments : IRequest<ApiResponse>
    {
        public string? ChapterId { get; set; }
        public string SortBy { get; set; } = "created_at:desc";
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = int.MaxValue;
    }

    public class GetChapterCommentsHandler : IRequestHandler<GetChapterComments, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetChapterCommentsHandler(ICommentRepository commentRepo, IChapterRepository chapterRepo, IUserRepository userRepo, IMapper mapper)
        {
            _commentRepo = commentRepo;
            _chapterRepo = chapterRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetChapterComments request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Chapter not found."
                };
            }

            FindCreterias findCreterias = new()
            {
                Limit = request.Limit,
                Page = request.Page
            };

            var sortBy = SystemHelper.ParseSortCriteria(request.SortBy);

            var chapterCommentList = await _commentRepo.GetCommentsByChapterIdAsync(chapter.novel_id, request.ChapterId, findCreterias, sortBy);

            if (chapterCommentList == null || chapterCommentList.Count == 0)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No Chapter's comment found."
                };
            }

            var commentIds = chapterCommentList.Select(c => c.id).ToList();
            var replyCountMap = await _commentRepo.CountRepliesPerCommentAsync(commentIds);

            var userIds = chapterCommentList.Select(c => c.user_id).Distinct().ToList();
            var users = await _userRepo.GetUsersByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.id);

            var response = new List<CommentResponse>();

            foreach (var comment in chapterCommentList)
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
                Message = "Retrieved chapter comments successfully.",
                Data = response
            };
        }
    }
}