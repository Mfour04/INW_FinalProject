using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Commands
{
    public class UpdateLockChapterStatusCommand : IRequest<ApiResponse>
    {
        public List<string> ChapterIds { get; set; }
        public bool IsLocked { get; set; }
    }
    public class UpdateLockChapterHandler : IRequestHandler<UpdateLockChapterStatusCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterHelperService _chapterHelperService;
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        public UpdateLockChapterHandler(IChapterRepository chapterRepository, IMediator mediator
            , ICurrentUserService currentUserService, INovelRepository novelRepository
            , IChapterHelperService chapterHelperService, IUserRepository userRepository
            , IEmailService emailService)
        {
            _chapterRepository = chapterRepository;
            _currentUserService = currentUserService;
            _novelRepository = novelRepository;
            _chapterHelperService = chapterHelperService;
            _mediator = mediator;
            _userRepository = userRepository;
            _emailService = emailService;
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
            var chapters = await _chapterRepository.GetChaptersByIdsAsync(request.ChapterIds);
            if (chapters == null || !chapters.Any())
            {
                return new ApiResponse { Success = false, Message = "Chapter not found" };
            }

            // Fix: Iterate through the list of chapters to check if any chapter is locked
            if (request.IsLocked && chapters.Any(chapter => chapter.is_lock))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "One or more chapters are already locked."
                };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(chapters.First().novel_id);
            if (novel == null)
            {
                return new ApiResponse { Success = false, Message = "Novel not found" };
            }

            // Update lock status for all chapters in the list
            foreach (var chapter in chapters)
            {
                await _chapterRepository.UpdateLockChaptersStatus(request.ChapterIds, request.IsLocked);
            }

            var action = request.IsLocked ? "locked" : "unlocked";
            var notificationCommand = new SendNotificationToUserCommand
            {
                UserId = novel.author_id,
                SenderId = userId,
                ChapterId = chapters.First().id, // Assuming notification is for the first chapter
                NovelId = novel.id,
                Message = request.IsLocked
                    ? $"Chương truyện \"{chapters.First().title}\" của Tiểu thuyết \"{novel.title}\" của bạn đã bị khoá bởi quản trị viên do vi phạm quy định. Vui lòng kiểm tra gmail của bạn để có phương thức giải quyết với chúng tôi."
                    : $"Chương truyện \"{chapters.First().title}\" của Tiểu thuyết \"{novel.title}\" của bạn đã được mở khoá bởi quản trị viên.",
                Type = request.IsLocked ? NotificationType.LockChapter : NotificationType.UnLockChapter
            };

            var notificationResponse = await _mediator.Send(notificationCommand);

            bool signalRSuccess = notificationResponse.Success &&
                                  notificationResponse.Data is not null &&
                                  (bool)(notificationResponse.Data as dynamic).SignalRSent;
            var author = await _userRepository.GetById(novel.author_id);
            if (!string.IsNullOrWhiteSpace(author.email))
            {
                var emailSubject = request.IsLocked
                    ? $"Chương truyện \"{chapters.First().title}\" của tiểu thuyết \"{novel.title}\" đã bị khoá"
                    : $"Chương truyện \"{chapters.First().title}\" của tiểu thuyết \"{novel.title}\" đã được mở khoá";
                var emailMessage = $@"
                <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: {(request.IsLocked ? "#d32f2f" : "#388e3c")}; text-align: center;'>
                        {(request.IsLocked ? "⚠️ Chương truyện đã bị khoá" : " Chương truyện đã được mở khoá")}
                    </h2>
                    <p>Chào <strong>{author.displayname}</strong>,</p>
                    <p>
                        {(request.IsLocked
                                       ? $"Chương truyện <span style='color:#1976d2; font-weight:bold;'>{chapters.First().title}</span> của tiểu thuyết của bạn với tiêu đề <span style='color:#1976d2; font-weight:bold;'>{novel.title}</span> đã bị <span style='color:#d32f2f; font-weight:bold;'>khoá</span> bởi quản trị viên do vi phạm quy định."
                                       : $"Chương truyện <span style='color:#1976d2; font-weight:bold;'>{chapters.First().title}</span> của tiểu thuyết của bạn với tiêu đề <span style='color:#1976d2; font-weight:bold;'>{novel.title}</span> đã được <span style='color:#388e3c; font-weight:bold;'>mở khoá</span>. Bạn có thể tiếp tục cập nhật nội dung.")}
                    </p>
                    {(request.IsLocked
                                   ? "<p>👉 Vui lòng kiểm tra lại nội dung và liên hệ với chúng tôi thông qua email này để giải quyết vấn đề.</p>"
                                   : "")}
                    <br/>
                    <p style='margin-top:20px;'>Trân trọng,</p>
                    <p style='font-weight:bold; color:#1976d2;'>Đội ngũ quản trị Inkwave Library</p>
                </div>";
                await _emailService.SendEmailAsync(author.email, emailSubject, emailMessage);
            }

            return new ApiResponse
            {
                Success = true,
                Message = $"Chapters have been {action} successfully and affected users have been notified.",
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
