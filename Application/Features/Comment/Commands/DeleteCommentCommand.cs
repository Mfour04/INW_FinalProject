using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Comment.Commands
{
    public class DeleteCommentCommand : IRequest<ApiResponse>
    {
        public string CommentId { get; set; }
    }

    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;

        public DeleteCommentCommandHandler(
            ICommentRepository commentRepository,
            INovelRepository novelRepository,
            IChapterRepository chapterRepository)
        {
            _commentRepository = commentRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
        }

        public async Task<ApiResponse> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var existComment = await _commentRepository.GetCommentByIdAsync(request.CommentId);
            if (existComment == null)
            {
                return Fail("Comment not found.");
            }

            var deleted = await _commentRepository.DeleteCommentAsync(request.CommentId);
            if (!deleted)
            {
                return Fail("Failed to delete comment.");
            }

            if (!string.IsNullOrWhiteSpace(existComment.chapter_id))
            {
                await _chapterRepository.DecrementCommentsAsync(existComment.chapter_id);
            }
            else if (!string.IsNullOrWhiteSpace(existComment.novel_id))
            {
                await _novelRepository.DecrementCommentsAsync(existComment.novel_id);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Comment Deleted Successfully",
                Data = deleted
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
