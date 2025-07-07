using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IForumPostRepository _postRepo;

        public CreatePostCommentCommandHandler(
            IForumCommentRepository postCommentRepo,
            IForumPostRepository postRepo)
        {
            _postCommentRepo = postCommentRepo;
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(CreatePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.PostId) && string.IsNullOrEmpty(request.ParentCommentId))
                return Fail("Either PostId or ParentCommentId must be provided.");

            if (!string.IsNullOrEmpty(request.PostId))
            {
                var post = await _postRepo.GetByIdAsync(request.PostId);
                if (post == null)
                    return Fail("Post does not exist.");
            }

            if (!string.IsNullOrEmpty(request.ParentCommentId))
            {
                var parent = await _postCommentRepo.GetByIdAsync(request.ParentCommentId);

                if (parent == null)
                    return Fail("Parent comment not found.");

                if (!string.IsNullOrEmpty(parent.parent_comment_id))
                    return Fail("Only 1-level replies are allowed.");
            }

            ForumCommentEntity comment = new()
            {
                id = Shared.Helpers.SystemHelper.RandomId(),
                post_id = request.PostId,
                user_id = request.UserId,
                content = request.Content,
                parent_comment_id = request.ParentCommentId,
                like_count = 0,
                reply_count = 0,
                created_at = DateTime.Now.Ticks
            };

            await _postCommentRepo.CreateAsync(comment);

            if (!string.IsNullOrEmpty(comment.post_id))
            {
                await _postRepo.IncrementCommentsAsync(comment.post_id);
            }

            return new ApiResponse
            {
                Success = true,
                Message = string.IsNullOrEmpty(request.ParentCommentId)
                    ? "Comment created successfully."
                    : "Reply created successfully.",
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