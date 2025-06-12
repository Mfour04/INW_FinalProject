using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
    }

    public class UpdatePostCommentCommandHandler : IRequestHandler<UpdatePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _commentRepo;

        public UpdatePostCommentCommandHandler(IForumCommentRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        public async Task<ApiResponse> Handle(UpdatePostCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new ApiResponse { Success = false, Message = "Content cannot be empty." };

            var updated = await _commentRepo.GetByIdAsync(request.Id);

            if (updated == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found."
                };
            }

            if (updated.user_id != request.UserId)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User are not allowed to edit this comment."
                };
            }

            updated.content = request.Content;
            updated.updated_at = DateTime.Now.Ticks;

            await _commentRepo.UpdateAsync(request.Id, updated);

            return new ApiResponse
            {
                Success = true,
                Message = "Comment updated successfully.",
                Data = updated
            };
        }
    }
}