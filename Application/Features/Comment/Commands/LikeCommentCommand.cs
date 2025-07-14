using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Comment.Commands
{
    public class LikeCommentCommand : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
        public int Type { get; set; }
    }

    public class LikeCommentCommandHandler : IRequestHandler<LikeCommentCommand, ApiResponse>
    {
        private readonly ICommentLikeRepository _commentLikeRepo;
        private readonly ICommentRepository _commentRepo;

        public LikeCommentCommandHandler(
            ICommentLikeRepository commentLikeRepo,
            ICommentRepository commentRepo)
        {
            _commentLikeRepo = commentLikeRepo;
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse> Handle(LikeCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CommentId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Missing required fields: CommentId or UserId.");

            if (!Enum.TryParse<CommentType>(request.Type.ToString(), out var commentType)
                 || (commentType != CommentType.Novel && commentType != CommentType.Chapter))
            {
                return Fail("Invalid or unsupported comment type.");
            }

            var targetComment = await _commentRepo.GetCommentByIdAsync(request.CommentId);
            if (targetComment == null)
                return Fail("Comment does not exist.");

            var hasLiked = await _commentLikeRepo.HasUserLikedCommentAsync(request.CommentId, request.UserId);
            if (hasLiked)
                return Fail("User has already liked this comment.");

            var like = new CommentLikeEntity
            {
                id = SystemHelper.RandomId(),
                comment_id = request.CommentId,
                user_id = request.UserId,
                type = commentType,
                like_at = TimeHelper.NowTicks
            };

            await _commentLikeRepo.LikeCommentAsync(like);

            return new ApiResponse
            {
                Success = true,
                Message = "Like successfully.",
                Data = like
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}