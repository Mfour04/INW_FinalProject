using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Forum.Commands
{
    public class UpdatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
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
                return Fail("Nội dung không được để trống.");

            var comment = await _commentRepo.GetByIdAsync(request.Id);
            if (comment == null)
                return Fail("Không tìm thấy bình luận.");

            if (comment.user_id != request.UserId)
                return Fail("Bạn không có quyền chỉnh sửa bình luận này.");

            ForumCommentEntity updated = new()
            {
                content = request.Content
            };

            var success = await _commentRepo.UpdateAsync(request.Id, updated);
            if (!success)
                return Fail("Cập nhật bình luận thất bại.");

            return new ApiResponse
            {
                Success = true,
                Message = "Cập nhật bình luận thành công.",
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
