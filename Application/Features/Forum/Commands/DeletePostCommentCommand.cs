using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class DeletePostCommentCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommand, ApiResponse>
    {
        private readonly IForumCommentRepository _commentRepo;
        private readonly IForumPostRepository _postRepo;
        private readonly ICurrentUserService _currentUser;

        public DeletePostCommentCommandHandler(
            IForumCommentRepository commentRepo,
            IForumPostRepository postRepo,
            ICurrentUserService currentUser)
        {
            _commentRepo = commentRepo;
            _postRepo = postRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
        {
            var validation = await ValidateCommand(request.Id);
            if (!validation.IsValid)
                return validation.FailResponse!;

            var comment = validation.Comment!;

            var replyIds = await _commentRepo.GetReplyIdsByCommentIdAsync(comment.id);

            if (replyIds.Any())
                await _commentRepo.DeleteManyAsync(replyIds);

            var deleted = await _commentRepo.DeleteAsync(request.Id);
            if (!deleted)
                return Fail("Failed to delete the comment.");

            if (!string.IsNullOrWhiteSpace(comment.post_id))
            {
                var totalDeleted = 1 + replyIds.Count;
                await _postRepo.DecrementCommentsAsync(comment.post_id, totalDeleted);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Comment deleted successfully."
            };
        }

        private async Task<(bool IsValid, ApiResponse? FailResponse, ForumCommentEntity? Comment)> ValidateCommand(string commentId)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return (false, Fail("Comment not found."), null);

            if (!_currentUser.IsAdmin() && comment.user_id != _currentUser.UserId)
                return (false, Fail("User is not allowed to delete this comment."), null);

            return (true, null, comment);
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}