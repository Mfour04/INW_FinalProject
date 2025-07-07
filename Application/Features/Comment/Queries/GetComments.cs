using AutoMapper;
using Domain.Entities;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;

namespace Application.Features.Comment.Queries
{
    public class GetComments : IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = int.MaxValue;
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public bool IncludeReplies { get; set; } = true;
    }

    public class GetCommentsHandler : IRequestHandler<GetComments, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;

        public GetCommentsHandler(
            ICommentRepository commentRepository,
            IChapterRepository chapterRepository,
            INovelRepository novelRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _chapterRepository = chapterRepository;
            _novelRepository = novelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetComments request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new FindCreterias
            {
                Limit = request.Limit,
                Page = request.Page
            };

            List<CommentEntity> comments;
            if (!string.IsNullOrEmpty(request.ChapterId) && !string.IsNullOrEmpty(request.NovelId))
            {
                var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
                if (chapter == null || chapter.novel_id != request.NovelId)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Chapter not found or does not belong to the specified novel."
                    };
                }
                comments = await _commentRepository.GetCommentsByChapterIdAsync(findCreterias, request.ChapterId);
            }
            else if (!string.IsNullOrEmpty(request.NovelId))
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Novel not found."
                    };
                }
                comments = await _commentRepository.GetCommentsByNovelIdAsync(findCreterias, request.NovelId);
            }
            else
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Either ChapterId or NovelId must be provided."
                };
            }
            if (comments == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "No comments found."
                };
            }
            var commentResponse = _mapper.Map<List<CommentResponse>>(comments);
            List<CommentResponse> result;

            if (request.IncludeReplies)
            {
                result = BuildNestedComments(commentResponse);
            }
            else
            {
                result = commentResponse
                    .Where(c => string.IsNullOrEmpty(c.ParentCommentId))
                    .ToList();
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Comments retrieved successfully",
                Data = result
            };
        }

        private List<CommentResponse> BuildNestedComments(List<CommentResponse> allComments)
        {
            var commentDict = allComments.ToDictionary(c => c.Id, c => c);

            var parentComments = allComments
                .Where(c => string.IsNullOrEmpty(c.ParentCommentId))
                .ToList();

            foreach (var comment in allComments)
            {
                if (!string.IsNullOrEmpty(comment.ParentCommentId) &&
                    commentDict.TryGetValue(comment.ParentCommentId, out var parentComment))
                {
                    parentComment.Replies.Add(comment);
                }
            }

            foreach (var comment in allComments)
            {
                comment.Replies = comment.Replies.OrderBy(r => r.CreatedAt).ToList();
            }

            return parentComments.OrderBy(c => c.CreatedAt).ToList();
        }
    }
}
