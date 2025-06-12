using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _postCommentRepo;

        public CreatePostCommentCommandHandler(IForumCommentRepository postCommentRepo)
        {
            _postCommentRepo = postCommentRepo;
        }

        public async Task<ApiResponse> Handle(CreatePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.PostId) && string.IsNullOrEmpty(request.ParentCommentId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Either PostId or ParentCommentId must be provided."
                };
            }

            if (!string.IsNullOrEmpty(request.ParentCommentId))
            {
                var parent = await _postCommentRepo.GetByIdAsync(request.ParentCommentId);

                if (parent == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Parent comment not found."
                    };
                }

                if (!string.IsNullOrEmpty(parent.parent_comment_id))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Only 1-level replies are allowed."
                    };
                }
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

            return new ApiResponse
            {
                Success = true,
                Message = string.IsNullOrEmpty(request.ParentCommentId)
                    ? "Comment created successfully."
                    : "Reply created successfully.",
                Data = comment
            };
        }
    }
}