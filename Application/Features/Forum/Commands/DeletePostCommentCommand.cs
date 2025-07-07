using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class DeletePostCommentCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
        public string UserId { get; set; }
    }

    public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _commentRepo;
        private readonly IForumPostRepository _postRepo;

        public DeletePostCommentCommandHandler(
            IForumCommentRepository commentRepo,
            IForumPostRepository postRepo)
        {
            _commentRepo = commentRepo;
            _postRepo = postRepo;
        }

        public async Task<ApiResponse> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepo.GetByIdAsync(request.Id);
            if (comment == null)
                return Fail("Comment not found.");

            if (comment.user_id != request.UserId)
                return Fail("User is not allowed to delete this comment.");

            var deleted = await _commentRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Failed to delete the comment.");

            if (!string.IsNullOrWhiteSpace(comment.post_id))
            {
                await _postRepo.DecrementCommentsAsync(comment.post_id);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Comment deleted successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}