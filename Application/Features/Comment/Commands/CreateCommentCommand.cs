using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Comment.Commands
{
    public class CreateCommentCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public string NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;

        public CreateCommentCommandHandler(
            ICommentRepository commentRepository,
            IChapterRepository chapterRepository,
            INovelRepository novelRepository)
        {
            _commentRepository = commentRepository;
            _chapterRepository = chapterRepository;
            _novelRepository = novelRepository;
        }

        public async Task<ApiResponse> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
                return Fail("NovelId is required.");

            if (string.IsNullOrWhiteSpace(request.Content))
                return Fail("Content is required.");

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return Fail("Novel not found.");

            if (!string.IsNullOrWhiteSpace(request.ChapterId))
            {
                var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
                if (chapter == null)
                    return Fail("Chapter not found.");

                if (chapter.novel_id != novel.id)
                    return Fail("Chapter does not belong to the specified novel.");
            }

            var comment = new CommentEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                chapter_id = request.ChapterId,
                user_id = request.UserId,
                content = request.Content,
                parent_comment_id = request.ParentCommentId,
                created_at = DateTime.UtcNow.Ticks
            };

            await _commentRepository.CreateCommentAsync(comment);

            if (!string.IsNullOrWhiteSpace(request.ChapterId))
            {
                await _chapterRepository.IncrementCommentsAsync(request.ChapterId);
            }
            else
            {
                await _novelRepository.IncrementCommentsAsync(request.NovelId);
            }
            
            return new ApiResponse
            {
                Success = true,
                Message = "Comment created successfully",
                Data = comment
			};
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
