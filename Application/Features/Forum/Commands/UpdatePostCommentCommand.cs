using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

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
                return Fail("Content cannot be empty.");

            var comment = await _commentRepo.GetByIdAsync(request.Id);
            if (comment == null)
                return Fail("Comment not found.");

            if (comment.user_id != request.UserId)
                return Fail("You are not allowed to edit this comment.");

            comment.content = request.Content;
            comment.updated_at = TimeHelper.NowTicks;

            var success = await _commentRepo.UpdateAsync(request.Id, comment);
            if (!success)
                return Fail("Failed to update the comment.");

            return new ApiResponse
            {
                Success = true,
                Message = "Comment updated successfully.",
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
