using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
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
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IChapterHelperService _chapterHelperService;
        public SendNotificationToUserHandler(INotificationRepository notificationRepository, INotificationService notificationService
            , IUserRepository userRepository, INovelRepository novelRepository, IChapterRepository chapterRepository
            , ICommentRepository commentRepository, IChapterHelperService chapterHelperService)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _commentRepository = commentRepository;
            _chapterHelperService = chapterHelperService;
        }
        public async Task<ApiResponse> Handle(SendNotificationToUserCommand request, CancellationToken cancellationToken)
        {
            // Lấy người gửi
            var senderUser = await _userRepository.GetById(request.SenderId);
            if (senderUser == null)
                return new ApiResponse { Success = false, Message = "Unauthorized. You must be login." };
            NovelEntity novel = null;
            ChapterEntity chapter = null;
            CommentEntity comment = null;
            CommentEntity replycomment = null;
            if (request.Type is NotificationType.ChapterReportNotification
                or NotificationType.NovelReportNofitication
                or NotificationType.CommentChapterNotification
                or NotificationType.CommentNovelNotification
                or NotificationType.ReportComment
                or NotificationType.RelyCommentNovel
                or NotificationType.RelyCommentChapter)
            {
                if (!string.IsNullOrWhiteSpace(request.NovelId))
                {
                    novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                    if (novel == null)
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Not found this novel."
                        };
                }

                if (!string.IsNullOrWhiteSpace(request.ChapterId))
                {
                    chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
                    if (chapter == null)
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Not found this chapter."
                        };
                }

                if (!string.IsNullOrWhiteSpace(request.CommentId))
                {
                    comment = await _commentRepository.GetByIdAsync(request.CommentId);
                    if (comment == null)
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Not found this Comment."
                        };
                }
                if (!string.IsNullOrWhiteSpace(request.ParentCommentId))
                {
                    replycomment = await _commentRepository.GetByIdAsync(request.ParentCommentId);
                    if (replycomment == null)
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Not found this Comment."
                        };
                }
            }
            var admin = await _userRepository.GetFirstUserByRoleAsync(Role.Admin);
            string senderName = senderUser.displayname;
            string receiverId = request.UserId; // fallback nếu cần dùng trực tiếp
            string novelTitle = null;
            string chapterTitle = null;
            string userNameReported = null;
            // Lấy receiver theo loại thông báo
            switch (request.Type)
            {
                case NotificationType.CommentNovelNotification:
                    receiverId = novel.author_id;
                    novelTitle = novel.title;
                    break;
                case NotificationType.NovelReportNofitication:               
                    receiverId = admin.id;
                    novelTitle = novel.title;
                    break;
                case NotificationType.CommentChapterNotification:
                    var author = await _chapterHelperService.GetChapterAuthorIdAsync(request.ChapterId);
                    receiverId = author;
                    chapterTitle = chapter.title;
                    novelTitle = novel.title;
                    break;
                case NotificationType.ChapterReportNotification:                 
                    var chapterNovel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
                    if (chapterNovel == null)
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Novel of this chapter not found."
                        };
                    receiverId = admin.id;
                    chapterTitle = chapter.title;
                    novelTitle = chapterNovel.title;
                    break;
                case NotificationType.RelyCommentNovel:
                    receiverId = replycomment.user_id;
                    if (receiverId == request.SenderId)
                    {
                        // Không gửi nếu tự reply chính mình
                        return new ApiResponse { Success = false, Message = "Cannot send notification when reply to yourself." };
                    }
                    chapterTitle = chapter?.title;
                    novelTitle = novel?.title;
                    break;

                case NotificationType.RelyCommentChapter:
                    receiverId = replycomment.user_id;
                    if (receiverId == request.SenderId)
                    {
                        // Không gửi nếu tự reply chính mình
                        return new ApiResponse { Success = false, Message = "Cannot send notification when reply to yourself." };
                    }
                    chapterTitle = chapter?.title;
                    novelTitle = novel?.title;
                    break;

                case NotificationType.LikeNovelComment:
                case NotificationType.LikeChapterComment:
                case NotificationType.ReportComment:                
                    receiverId = admin.id;
                    break;
                case NotificationType.UserReport:
                    var reportedUser = await _userRepository.GetById(request.UserReportedId);
                    if (reportedUser == null)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Reported user not found."
                        };
                    }
                    receiverId = admin.id;
                    userNameReported = reportedUser.username;
                    break;
                default:
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "This notification type is not supported."
                    };
            }

            if (receiverId == null || receiverId == request.SenderId)
            return new ApiResponse { Success = false, Message = "Cannot send notification to yourself." };

            // Lấy message nếu chưa có
            string message = !string.IsNullOrWhiteSpace(request.Message)
                ? request.Message
                : GenerateMessage(request.Type, senderName, novelTitle, chapterTitle, userNameReported);

            var notification = new NotificationEntity
            {
                id = SystemHelper.RandomId(),
                user_id = receiverId,
                message = message,
                type = request.Type,
                is_read = false,
                created_at = DateTime.UtcNow.Ticks
            };

            await _notificationRepository.CreateAsync(notification);
            await _notificationService.SendNotificationAsync(receiverId, message, request.Type);
            return new ApiResponse
            {
                Success = true,
                Message = "Notification sent successfully",
                Data = new
                {
                    Receiver = receiverId,
                    NotificationMessage = message,
                    SignalRSent = true
                }
            };
        }

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
    