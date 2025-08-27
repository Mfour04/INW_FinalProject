using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.SignalRHub;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Notification.Commands
{
    public class SendNotificationToUserCommand: IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string SenderId { get; set; }
        public string UserReportedId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string CommentId { get; set; }
        public string ParentCommentId { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
    }

    public class SendNotificationToUserHandler : IRequestHandler<SendNotificationToUserCommand, ApiResponse>
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IChapterHelperService _chapterHelperService;

        public SendNotificationToUserHandler(
            INotificationService notificationService,
            IUserRepository userRepository,
            INovelRepository novelRepository,
            IChapterRepository chapterRepository,
            ICommentRepository commentRepository,
            IChapterHelperService chapterHelperService)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _commentRepository = commentRepository;
            _chapterHelperService = chapterHelperService;
        }

        public async Task<ApiResponse> Handle(SendNotificationToUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate sender
            var senderUser = await _userRepository.GetById(request.SenderId);
            if (senderUser == null)
                return Fail("Không được phép. Bạn phải đăng nhập.");

            var senderName = senderUser.displayname;
            var receiverId = request.UserId;
            string novelTitle = null;
            string chapterTitle = null;
            string userNameReported = null;

            var admin = await _userRepository.GetFirstUserByRoleAsync(Role.Admin);

            // 2. Xác định receiver và load data tùy theo loại notification
            switch (request.Type)
            {
                case NotificationType.CommentNovelNotification:
                    var novelComment = await RequireNovel(request.NovelId);
                    if (novelComment == null) return Fail("Không tìm thấy tiểu thuyết.");
                    receiverId = novelComment.author_id;
                    novelTitle = novelComment.title;
                    break;

                case NotificationType.NovelReportNofitication:
                    var novelReport = await RequireNovel(request.NovelId);
                    if (novelReport == null) return Fail("Không tìm thấy tiểu thuyết.");
                    receiverId = admin.id;
                    novelTitle = novelReport.title;
                    break;

                case NotificationType.CommentChapterNotification:
                    var chapterComment = await RequireChapter(request.ChapterId);
                    if (chapterComment == null) return Fail("Không tìm thấy chương.");
                    var novelOfChapterComment = await RequireNovel(chapterComment.novel_id);
                    if (novelOfChapterComment == null) return Fail("Không tìm thấy tiểu thuyết của chương này.");
                    receiverId = await _chapterHelperService.GetChapterAuthorIdAsync(request.ChapterId);
                    chapterTitle = chapterComment.title;
                    novelTitle = novelOfChapterComment.title;
                    break;

                case NotificationType.ChapterReportNotification:
                    var chapterReport = await RequireChapter(request.ChapterId);
                    if (chapterReport == null) return Fail("Không tìm thấy chương.");
                    var novelOfChapterReport = await RequireNovel(chapterReport.novel_id);
                    if (novelOfChapterReport == null) return Fail("Không tìm thấy tiểu thuyết của chương này.");
                    receiverId = admin.id;
                    chapterTitle = chapterReport.title;
                    novelTitle = novelOfChapterReport.title;
                    break;

                case NotificationType.RelyCommentNovel:
                case NotificationType.RelyCommentChapter:
                    var replyComment = await RequireComment(request.ParentCommentId);
                    if (replyComment == null) return Fail("Không tìm thấy bình luận.");
                    if (replyComment.user_id == request.SenderId)
                        return Fail("Không thể gửi thông báo khi trả lời chính mình.");
                    receiverId = replyComment.user_id;
                    if (!string.IsNullOrWhiteSpace(request.NovelId))
                    {
                        var novelReply = await RequireNovel(request.NovelId);
                        novelTitle = novelReply?.title;
                    }
                    if (!string.IsNullOrWhiteSpace(request.ChapterId))
                    {
                        var chapterReply = await RequireChapter(request.ChapterId);
                        chapterTitle = chapterReply?.title;
                    }
                    break;

                case NotificationType.LikeNovelComment:
                case NotificationType.LikeChapterComment:
                case NotificationType.ReportComment:
                    receiverId = admin.id;
                    break;

                case NotificationType.UserReport:
                    var reportedUser = await _userRepository.GetById(request.UserReportedId);
                    if (reportedUser == null) return Fail("Không tìm thấy người dùng được báo cáo.");
                    receiverId = admin.id;
                    userNameReported = reportedUser.username;
                    break;

                case NotificationType.BanUser:
                case NotificationType.UnBanUser:
                case NotificationType.LockNovel:
                case NotificationType.UnLockNovel:
                    var novelLock = await RequireNovel(request.NovelId);
                    receiverId = novelLock?.author_id ?? request.UserId;
                    break;

                case NotificationType.LockChapter:
                case NotificationType.UnLockChapter:
                    receiverId = await _chapterHelperService.GetChapterAuthorIdAsync(request.ChapterId);
                    break;
                default:
                    return Fail("Loại thông báo này không được hỗ trợ.");
            }

            if (receiverId == null || receiverId == request.SenderId)
                return Fail("Không thể gửi thông báo cho chính mình.");

            // 3. Tạo message
            var message = string.IsNullOrWhiteSpace(request.Message)
                ? GenerateMessage(request.Type, senderName, novelTitle, chapterTitle, userNameReported)
                : request.Message;

            // 4. Gọi NotificationService (tự lưu DB + gửi SignalR)
            var notifications = await _notificationService.SendNotificationToUsersAsync(
                new[] { receiverId }, // chỉ 1 user
                message,
                request.Type
            );

            bool success = notifications.Any();

            return new ApiResponse
            {
                Success = success,
                Message = success ? "Thông báo đã được gửi thành công": "Không gửi được thông báo",
                Data = new
                {
                    Receiver = receiverId,
                    NotificationMessage = message,
                    SignalRSent = success,
                    Notifications = notifications // trả nguyên payload list từ service
                }
            };

        }

        private ApiResponse Fail(string message) =>
            new ApiResponse { Success = false, Message = message };

        private async Task<NovelEntity> RequireNovel(string novelId) =>
            string.IsNullOrWhiteSpace(novelId) ? null : await _novelRepository.GetByNovelIdAsync(novelId);

        private async Task<ChapterEntity> RequireChapter(string chapterId) =>
            string.IsNullOrWhiteSpace(chapterId) ? null : await _chapterRepository.GetByIdAsync(chapterId);

        private async Task<CommentEntity> RequireComment(string commentId) =>
            string.IsNullOrWhiteSpace(commentId) ? null : await _commentRepository.GetByIdAsync(commentId);

        public static string GenerateMessage(NotificationType type, string senderUserName, string novelTitle = null, string chapterTitle = null, string userNameReported = null)
        {
            return type switch
            {
                NotificationType.CommentNovelNotification => $"{senderUserName} đã bình luận vào tiểu thuyết \"{novelTitle}\" của bạn.",
                NotificationType.CommentChapterNotification => $"{senderUserName} đã bình luận vào chương \"{chapterTitle}\" của tiểu thuyết \"{novelTitle}\" của bạn.",
                NotificationType.RelyCommentNovel => $"{senderUserName} đã phản hồi bình luận của bạn trong tiểu thuyết \"{novelTitle}\".",
                NotificationType.RelyCommentChapter => $"{senderUserName} đã phản hồi bình luận của bạn trong chương \"{chapterTitle}\".",
                NotificationType.LikeNovelComment => $"{senderUserName} đã thích bình luận của bạn trong tiểu thuyết.",
                NotificationType.LikeChapterComment => $"{senderUserName} đã thích bình luận của bạn trong chương.",
                NotificationType.ReportComment => $"{senderUserName} đã báo cáo một bình luận.",
                NotificationType.NovelReportNofitication => $"{senderUserName} đã báo cáo tiểu thuyết \"{novelTitle}\".",
                NotificationType.ChapterReportNotification => $"{senderUserName} đã báo cáo chương \"{chapterTitle}\" của tiểu thuyết  \"{novelTitle}\".",
                NotificationType.UserReport => $"{senderUserName} đã báo cáo người dùng {userNameReported} đã vi phạm điều luật",
                _ => "Bạn có một thông báo mới."
            };
        }
    }

}
