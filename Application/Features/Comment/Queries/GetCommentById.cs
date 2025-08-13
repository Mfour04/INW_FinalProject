using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;

namespace Application.Features.Comment.Queries
{
    public class GetCommentById : IRequest<ApiResponse>
    {
        public string? CommentId { get; set; }
    }

    public class GetCommentByIdHandler : IRequestHandler<GetCommentById, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetCommentByIdHandler(ICommentRepository commentRepo, IUserRepository userRepo, IMapper mapper)
        {
            _commentRepo = commentRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetCommentById request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepo.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Comment not found."
                };
            }

            var response = _mapper.Map<CommentResponse>(comment);

            var replyCountMap = await _commentRepo.CountRepliesPerCommentAsync(new List<string> { comment.id });
            response.ReplyCount = replyCountMap.TryGetValue(comment.id, out var count) ? count : 0;

            var user = await _userRepo.GetById(comment.user_id);
            response.Author = new BaseCommentResponse.UserInfo
            {
                Id = user.id,
                Username = user.username,
                DisplayName = user.displayname,
                Avatar = user.avata_url
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Comment retrieved successfully.",
                Data = response
            };
        }
    }
}
