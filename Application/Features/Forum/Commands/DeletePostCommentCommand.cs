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

        public DeletePostCommentCommandHandler(IForumCommentRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepo.GetByIdAsync(request.Id);

            if (comment == null)
                return new ApiResponse { Success = false, Message = "Comment not found." };

            if (comment.user_id != request.UserId)
                return new ApiResponse { Success = false, Message = "User are not allowed to delete this comment." };

            var isSuccess = await _commentRepo.DeleteAsync(request.Id);

            if (!isSuccess)
                return new ApiResponse { Success = false, Message = "Failed to delete the post or post not found." };

            return new ApiResponse
            {
                Success = true,
                Message = "Post deleted successfully.",
            };
        }
    }
}