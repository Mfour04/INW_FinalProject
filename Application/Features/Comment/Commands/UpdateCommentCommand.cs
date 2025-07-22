using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;

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
        private readonly IMapper _mapper;

        public UpdateCommentHandler(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.CommentId!);
            if (comment == null)
                return new ApiResponse { Success = false, Message = "Comment not found." };

            if (comment.user_id != request.UserId)
                return new ApiResponse { Success = false, Message = "You are not authorized to update this comment." };

            comment.content = request.Content;
            comment.updated_at = TimeHelper.NowTicks;

            await _commentRepository.UpdateAsync(comment);

            var response = _mapper.Map<UpdateCommentResponse>(comment);

            return new ApiResponse
            {
                Success = true,
                Message = "Comment Updated Successfully",
                Data = response
            };
        }
    }
}
