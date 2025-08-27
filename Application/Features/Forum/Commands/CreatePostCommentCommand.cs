using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Forum;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class CreatePostCommentCommand : IRequest<ApiResponse>
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
        public required string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommand, ApiResponse>
    {
        private readonly IMapper _mapper;
        private readonly IForumCommentRepository _postCommentRepo;
        private readonly IForumPostRepository _postRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notificationService;

        public CreatePostCommentCommandHandler(
            IMapper mapper,
            IForumCommentRepository postCommentRepo,
            IForumPostRepository postRepo,
            IUserRepository userRepo,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _postCommentRepo = postCommentRepo;
            _postRepo = postRepo;
            _userRepo = userRepo;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse> Handle(CreatePostCommentCommand request, CancellationToken cancellationToken)
        {
            var validation = await ValidateCommand(request);
            if (validation != null)
                return validation;

            var comment = new ForumCommentEntity
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

            var created = await _postCommentRepo.CreateAsync(comment);
            if (created == null)
                return Fail("Tạo bình luận thất bại");

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

            if (!string.IsNullOrEmpty(comment.parent_comment_id))
            {
                await _postCommentRepo.IncrementReplyCountAsync(comment.parent_comment_id);

                var parentComment = await _postCommentRepo.GetByIdAsync(comment.parent_comment_id);
                if (parentComment != null && !string.IsNullOrEmpty(parentComment.post_id))
                {
                    var incResult = await IncrementCommentCount(parentComment.post_id);
                    if (incResult != null) return incResult;
                    if (!string.IsNullOrWhiteSpace(parentComment.user_id) && parentComment.user_id != request.UserId)
                    {
                        string message = $"{user?.displayname ?? "Ai đó"} đã trả lời bình luận của bạn: \"{request.Content}\"";
                        await _notificationService.SendNotificationToUsersAsync(
                            new[] { parentComment.user_id },
                            message,
                            NotificationType.RelyCommentPost
                        );
                    }
                }
            }
            else if (!string.IsNullOrEmpty(comment.post_id))
            {
                var incResult = await IncrementCommentCount(comment.post_id);
                if (incResult != null) return incResult;
                var post = await _postRepo.GetByIdAsync(comment.post_id);
                if (post != null && !string.IsNullOrWhiteSpace(post.user_id) && post.user_id != request.UserId)
                {
                    string message = $"{user?.displayname} đã bình luận bài viết của bạn: \"{request.Content}\"";
                    await _notificationService.SendNotificationToUsersAsync(
                        new[] { post.user_id },
                        message,
                        NotificationType.CommentPostCreated
                    );
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = string.IsNullOrEmpty(request.ParentCommentId)
                    ? "Tạo bình luận thành công."
                    : "Trả lời bình luận thành công.",
                Data = response
            };
        }

        private async Task<ApiResponse?> ValidateCommand(CreatePostCommentCommand request)
        {
            bool hasPostId = !string.IsNullOrEmpty(request.PostId);
            bool hasParentId = !string.IsNullOrEmpty(request.ParentCommentId);

            if (!(hasPostId ^ hasParentId))
                return Fail("Phải cung cấp PostId hoặc ParentCommentId, nhưng không được cung cấp cả hai.");

            if (hasPostId)
            {
                var post = await _postRepo.GetByIdAsync(request.PostId);
                if (post == null) return Fail("Bài viết không tồn tại");
            }

            if (hasParentId)
            {
                var parentComment = await _postCommentRepo.GetByIdAsync(request.ParentCommentId);
                if (parentComment == null) return Fail("Không tìm thấy parent comment");
                if (!string.IsNullOrEmpty(parentComment.parent_comment_id))
                    return Fail("Chỉ được phép trả lời ở mức 1.");
            }

            return null;
        }

        private async Task<ApiResponse?> IncrementCommentCount(string postId)
        {
            var success = await _postRepo.IncrementCommentsAsync(postId);
            return success ? null : Fail("Không cập nhật được số lượng bình luận bài viết.");
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
