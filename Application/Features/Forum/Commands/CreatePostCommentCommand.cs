using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommand, ApiResponse>
    {
        private readonly IMapper _mapper;
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IForumPostRepository _postRepo;
        private readonly IUserRepository _userRepo;

        public CreatePostCommentCommandHandler(
            IMapper mapper,
            IForumCommentRepository postCommentRepo,
            IForumPostRepository postRepo,
            IUserRepository userRepo)
        {
            _mapper = mapper;
            _postCommentRepo = postCommentRepo;
            _postRepo = postRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse> Handle(CreatePostCommentCommand request, CancellationToken cancellationToken)
        {
            bool hasPostId = !string.IsNullOrEmpty(request.PostId);
            bool hasParentId = !string.IsNullOrEmpty(request.ParentCommentId);

            if (!(hasPostId ^ hasParentId))
                return Fail("Either PostId or ParentCommentId must be provided, but not both.");

            if (!string.IsNullOrEmpty(request.PostId))
            {
                var post = await _postRepo.GetByIdAsync(request.PostId);
                if (post == null)
                    return Fail("Post does not exist.");
            }

            if (!string.IsNullOrEmpty(request.ParentCommentId))
            {
                var parent = await _postCommentRepo.GetByIdAsync(request.ParentCommentId);

                if (parent == null)
                    return Fail("Parent comment not found.");

                if (!string.IsNullOrEmpty(parent.parent_comment_id))
                    return Fail("Only 1-level replies are allowed.");
            }

            ForumCommentEntity comment = new()
            {
                id = SystemHelper.RandomId(),
                post_id = request.PostId,
                user_id = request.UserId,
                content = request.Content,
                parent_comment_id = request.ParentCommentId,
                like_count = 0,
                reply_count = 0,
                created_at = TimeHelper.NowTicks
            };

            await _postCommentRepo.CreateAsync(comment);

            var response = _mapper.Map<PostCommentCreatedResponse>(comment);

            var user = await _userRepo.GetById(request.UserId);
            response.Author = user != null
                ? new BasePostCommentResponse.PostCommentAuthorResponse
                {
                    Id = user.id,
                    Username = user.username,
                    Avatar = user.avata_url
                }
                : new BasePostCommentResponse.PostCommentAuthorResponse();

            if (!string.IsNullOrEmpty(comment.post_id))
            {
                await _postRepo.IncrementCommentsAsync(comment.post_id);
            }

            return new ApiResponse
            {
                Success = true,
                Message = string.IsNullOrEmpty(request.ParentCommentId)
                    ? "Comment created successfully."
                    : "Reply created successfully.",
                Data = response
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}