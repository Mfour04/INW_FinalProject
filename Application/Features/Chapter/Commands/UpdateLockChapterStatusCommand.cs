using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Commands
{
    public class UpdateLockChapterStatusCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public bool IsLocked { get; set; }
    }
    public class UpdateLockChapterHandler : IRequestHandler<UpdateLockChapterStatusCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterHelperService _chapterHelperService;
        private readonly IMediator _mediator;
        public UpdateLockChapterHandler(IChapterRepository chapterRepository, IMediator mediator
            , ICurrentUserService currentUserService, INovelRepository novelRepository, IChapterHelperService chapterHelperService)
        {
            _chapterRepository = chapterRepository;
            _currentUserService = currentUserService;
            _novelRepository = novelRepository;
            _chapterHelperService = chapterHelperService;
            _mediator = mediator;
        }
        public async Task<ApiResponse> Handle(UpdateLockChapterStatusCommand request, CancellationToken cancellationToken)
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
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                return new ApiResponse { Success = false, Message = "Chapter not found" };
            }
            if (request.IsLocked && chapter.is_lock)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Chapter is already locked."
                };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }

            await _chapterRepository.UpdateLockChapterStatus(request.ChapterId, request.IsLocked);
            var action = request.IsLocked ? "locked" : "unlocked";
            var notificationCommand = new SendNotificationToUserCommand
            {
                UserId = novel.author_id,
                SenderId = userId,
                ChapterId = chapter.id,
                NovelId = novel.id,
                Message = request.IsLocked
                ? $"Chương truyện \"{chapter.title}\" của Tiểu thuyết \"{novel.title}\" của bạn đã bị khoá bởi quản trị viên do vi phạm quy định. Vui lòng kiểm tra gmail của bạn để có phương thức giải quyết với chúng tôi."
                : $"Chương truyện \"{chapter.title}\" của Tiểu thuyết \"{novel.title}\" của bạn đã được mở khoá bởi quản trị viên.",
                Type = request.IsLocked ? NotificationType.LockChapter : NotificationType.UnLockChapter
            };

            var notificationResponse = await _mediator.Send(notificationCommand);

            bool signalRSuccess = notificationResponse.Success &&
                          notificationResponse.Data is not null &&
                          (bool)(notificationResponse.Data as dynamic).SignalRSent;
            return new ApiResponse
            {
                Success = true,
                Message = $"Chapter has been {action} successfully and affected users have been notified.",
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
