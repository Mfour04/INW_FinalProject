using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;

namespace Application.Features.Comment.Commands
{
    public class UpdateCommentCommand : IRequest<ApiResponse>
    {
        public UpdateCommentResponse UpdateComment { get; set; }
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
            var input = request.UpdateComment;
            var comment = await _commentRepository.GetCommentByIdAsync(input.CommentId);
            if (comment == null)
                return new ApiResponse { Success = false, Message = "Comment not found" };
            comment.content = input.Content ?? comment.content;
            await _commentRepository.UpdateCommentAsync(comment);
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
