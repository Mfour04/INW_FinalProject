using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Forum.Commands
{
    public class LikePostCommand : IRequest<ApiResponse>
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
    }

    public class LikePostCommandHandler : IRequestHandler<LikePostCommand, ApiResponse>
    {
        private readonly IForumPostLikeRepository _postLikeRepo;
        private readonly IForumPostRepository _postRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepo;
        public LikePostCommandHandler(
            IForumPostLikeRepository postLikeRepo,
            IForumPostRepository postRepo,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _postLikeRepo = postLikeRepo;
            _postRepo = postRepo;
            _notificationService = notificationService;
            _userRepo = userRepository;
        }

        public async Task<ApiResponse> Handle(LikePostCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PostId) || string.IsNullOrWhiteSpace(request.UserId))
                return Fail("Missing required fields: PostId or UserId.");

            var post = await _postRepo.GetByIdAsync(request.PostId);
            if (post == null)
                return Fail("Post does not exist.");

            var hasLiked = await _postLikeRepo.HasUserLikedPostAsync(request.PostId, request.UserId);
            if (hasLiked)
                return Fail("User has already liked this post.");

            var like = new ForumPostLikeEntity
            {
                id = SystemHelper.RandomId(),
                post_id = request.PostId,
                user_id = request.UserId,
                like_at = TimeHelper.NowTicks
            };

            await _postLikeRepo.LikePostAsync(like);
            await _postRepo.IncrementLikesAsync(request.PostId);
            if (!string.IsNullOrEmpty(post.user_id) && post.user_id != request.UserId)
            {
                var liker = await _userRepo.GetById(request.UserId);
                string message = $"{liker.displayname} đã thích bài viết của bạn.";
                await _notificationService.SendNotificationToUsersAsync(
                    new[] { post.user_id },
                    message,
                    NotificationType.LikePostCreated
                );
            }
            return new ApiResponse
            {
                Success = true,
                Message = "Like successfully.",
            };
        }

        private ApiResponse Fail(string msg) => new()
        {
            Success = false,
            Message = msg
        };
    }
}
