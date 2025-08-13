using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Notification;

namespace Application.Features.Novel.Commands
{
    public class UpdateLockNovelCommand: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public bool IsLocked { get; set; }
    }
    public class UpdateLockNovelHandler : IRequestHandler<UpdateLockNovelCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;   
        public UpdateLockNovelHandler(INovelRepository novelRepository, ICurrentUserService currentUserService
            , IMediator mediator)
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }
        public async Task<ApiResponse> Handle(UpdateLockNovelCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return new ApiResponse { Success = false, Message = "Unauthorized" };
            }
            if (!_currentUserService.IsAdmin())
            {
                return new ApiResponse { Success = false, Message = "Forbidden: Admin role required" };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }
            if (request.IsLocked && novel.is_lock)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Chapter is already locked."
                };
            }
            await _novelRepository.UpdateLockStatusAsync(request.NovelId, request.IsLocked);
            var action = request.IsLocked ? "locked" : "unlocked";
            var notificationCommand = new SendNotificationToUserCommand
            {
                UserId = novel.author_id,
                SenderId = userId,
                NovelId = novel.id,
                Message = request.IsLocked
                ? $"Tiểu thuyết \"{novel.title}\" của bạn đã bị khoá bởi quản trị viên do vi phạm quy định. Vui lòng kiểm tra gmail của bạn để có phương thức giải quyết với chúng tôi."
                : $"Tiểu thuyết \"{novel.title}\" của bạn đã được mở khoá bởi quản trị viên.",
                Type = request.IsLocked ? NotificationType.LockNovel : NotificationType.UnLockNovel
            };

            var notificationResponse = await _mediator.Send(notificationCommand);

            bool signalRSuccess = notificationResponse.Success &&
                          notificationResponse.Data is not null &&
                          (bool)(notificationResponse.Data as dynamic).SignalRSent;
            return new ApiResponse
            {
                Success = true,
                Message = $"Novel has been {action} successfully and affected users have been notified.",
                Data = new
                {
                    NovelId = novel.id,
                    AuthorId = novel.author_id,
                    SignalRSent = signalRSuccess
                }
            };
        }
    }
}
