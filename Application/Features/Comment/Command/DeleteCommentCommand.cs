using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Comment.Command
{
    public class DeleteCommentCommand : IRequest<ApiResponse>
    {
        public string CommentId { get; set; }
    }

    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;

        public DeleteCommentCommandHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<ApiResponse> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commentRepository.DeleteCommentAsync(request.CommentId);
            if (deleted == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found or already deleted"
                };
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Comment Deleted Successfully",
                Data = deleted
            };
        }
    }
}
