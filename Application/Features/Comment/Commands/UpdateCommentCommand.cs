using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Comment.Commands
{
    public class UpdateCommentCommand : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
    }

    public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;

        public UpdateCommentHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<ApiResponse> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.CommentId!);
            if (comment == null)
                return Fail("Comment not found.");

            if (comment.user_id != request.UserId)
                return Fail("You are not authorized to update this comment.");

            CommentEntity updated = new()
            {
                content = request.Content
            };

            var success = await _commentRepository.UpdateAsync(request.CommentId, updated);
            if (!success)
                return Fail("Failed to update the badge.");

            return new ApiResponse
            {
                Success = true,
                Message = "Comment Updated Successfully."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
