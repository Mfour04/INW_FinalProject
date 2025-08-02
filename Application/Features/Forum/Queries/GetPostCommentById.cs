using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;

namespace Application.Features.Forum.Queries
{
    public class GetPostCommentById : IRequest<ApiResponse>
    {
        public string Id { get; set; }
    }

    public class GetPostCommentByIdHandler : IRequestHandler<GetPostCommentById, ApiResponse>
    {
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public GetPostCommentByIdHandler(
            IForumCommentRepository postCommentRepo,
            IUserRepository userRepo,
            IMapper mapper)
        {
            _postCommentRepo = postCommentRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetPostCommentById request, CancellationToken cancellationToken)
        {
            var comment = await _postCommentRepo.GetByIdAsync(request.Id);

            if (comment == null)
            {
                return new ApiResponse { Success = false, Message = "No forum comment found." };
            }

            var response = _mapper.Map<BasePostCommentResponse>(comment);

            var user = await _userRepo.GetById(comment.user_id);
            if (user != null)
            {
                response.Author = new BasePostCommentResponse.PostCommentAuthorResponse
                {
                    Id = user.id,
                    Username = user.username,
                    DisplayName = user.displayname,
                    Avatar = user.avata_url
                };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Comment retrieved successfully.",
                Data = response
            };
        }
    }
}