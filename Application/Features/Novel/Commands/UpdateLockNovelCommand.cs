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
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        public UpdateLockNovelHandler(INovelRepository novelRepository, ICurrentUserService currentUserService
            , IMediator mediator, IEmailService emailService, IUserRepository userRepository)
        {
            _novelRepository = novelRepository;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _emailService = emailService;
            _userRepository = userRepository;
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
            var author = await _userRepository.GetById(novel.author_id);
            if (!string.IsNullOrWhiteSpace(author.email))
            {
                var emailSubject = request.IsLocked
                    ? $"Tiểu thuyết \"{novel.title}\" đã bị khoá"
                    : $"Tiểu thuyết \"{novel.title}\" đã được mở khoá";
                var emailMessage = $@"
            <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                <h2 style='color: {(request.IsLocked ? "#d32f2f" : "#388e3c")}; text-align: center;'>
                    {(request.IsLocked ? "⚠️ Tiểu thuyết đã bị khoá" : "✅ Tiểu thuyết đã được mở khoá")}
                </h2>
                <p>Chào <strong>{author.displayname}</strong>,</p>
                <p>
                    {(request.IsLocked
                       ? $"Tiểu thuyết của bạn với tiêu đề <span style='color:#1976d2; font-weight:bold;'>{novel.title}</span> đã bị <span style='color:#d32f2f; font-weight:bold;'>khoá</span> bởi quản trị viên do vi phạm quy định."
                       : $"Tiểu thuyết của bạn với tiêu đề <span style='color:#1976d2; font-weight:bold;'>{novel.title}</span> đã được <span style='color:#388e3c; font-weight:bold;'>mở khoá</span>. Bạn có thể tiếp tục cập nhật nội dung.")}
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
