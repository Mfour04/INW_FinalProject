using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Comment.Commands
{
    public class UnlikeChapterCommentCommand : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
    }

    public class UnlikeChapterCommentCommandHandler : IRequestHandler<UnlikeChapterCommentCommand, ApiResponse>
    {
        private readonly ICommentLikeRepository _commentLikeRepo;
        private readonly ICommentRepository _commentRepo;

        public UnlikeChapterCommentCommandHandler(
            ICommentLikeRepository commentLikeRepo,
              ICommentRepository commentRepo)
        {
            _commentLikeRepo = commentLikeRepo;
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse> Handle(UnlikeChapterCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CommentId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Missing required fields: CommentId or UserId.");

            var comment = await _commentRepo.GetCommentByIdAsync(request.CommentId);
            if (comment == null)
                return Fail("Comment does not exist.");

            var hasLiked = await _commentLikeRepo.HasUserLikedCommentAsync(request.CommentId, request.UserId);
            if (!hasLiked)
                return Fail("User has not liked this comment.");

            var isSuccess = await _commentLikeRepo.UnlikeCommentAsync(request.CommentId, request.UserId);
            if (!isSuccess)
                return Fail("Failed to unlike the comment.");

            return new ApiResponse
            {
                Success = true,
                Message = "Unlike successfully."
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}