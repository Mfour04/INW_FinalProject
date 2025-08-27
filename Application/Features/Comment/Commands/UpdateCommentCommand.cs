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
                return Fail("Không tìm thấy bình luận.");

            if (comment.user_id != request.UserId)
                return Fail("Bạn không có quyền cập nhật bình luận này.");

            CommentEntity updated = new()
            {
                content = request.Content
            };

            var success = await _commentRepository.UpdateAsync(request.CommentId, updated);
            if (!success)
                return Fail("Cập nhật bình luận thất bại.");

            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật bình luận thành công."
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
