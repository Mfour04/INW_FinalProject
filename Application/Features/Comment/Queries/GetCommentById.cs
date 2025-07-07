using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;

namespace Application.Features.Comment.Queries
{
    public class GetCommentById : IRequest<ApiResponse>
    {
        public string CommentId { get; set; }
    }

    public class GetCommentByIdHandler : IRequestHandler<GetCommentById, ApiResponse>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public GetCommentByIdHandler(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetCommentById request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(request.CommentId);
            if (comment == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found."
                };
            }
            var commentResponse = _mapper.Map<CommentResponse>(comment);

            return new ApiResponse
            {
                Success = true,
                Message = "Comment retrieved successfully.",
                Data = commentResponse
            };
        }
    }
}
