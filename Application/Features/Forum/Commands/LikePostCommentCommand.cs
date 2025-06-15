using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class LikePostCommentCommand : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
    }

    public class LikePostCommentCommandHandler : IRequestHandler<LikePostCommentCommand, ApiResponse>
    {
        private readonly ICommentLikeRepository _commentLikeRepo;
        private readonly IForumCommentRepository _postCommentRepo;

        public LikePostCommentCommandHandler(
            ICommentLikeRepository commentLikeRepo,
            IForumCommentRepository postCommentRepo)
        {
            _commentLikeRepo = commentLikeRepo;
            _postCommentRepo = postCommentRepo;
        }

        public async Task<ApiResponse> Handle(LikePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CommentId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Missing required fields: CommentId or UserId.");

            var targetComment = await _postCommentRepo.GetByIdAsync(request.CommentId);
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
                type = CommentType.Forum,
                like_at = DateTime.Now.Ticks
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
