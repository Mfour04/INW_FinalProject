using Application.Features.Notification.Commands;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Report;
using Shared.Helpers;
using System.Text.Json.Serialization;

namespace Application.Features.Report.Command
{
    public class CreateReportCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public string MemberId { get; set; }
        public string NovelId { get; set; }
        public string ChapterId { get; set; }
        public string CommentId { get; set; }
        public string ForumPostId { get; set; }
        public string ForumCommentId { get; set; }
        public ReportTypeStatus Type { get; set; }
        public string Reason { get; set; }
    }

    public class CreateReponseCommandHandler : IRequestHandler<CreateReportCommand, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IForumPostRepository _forumPostRepository;
        private readonly IForumCommentRepository _forumCommentRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateReponseCommandHandler(INovelRepository novelRepository, IChapterRepository chapterRepository, ICommentRepository commentRepository
            , IUserRepository userRepository, IReportRepository reportRepository, IForumPostRepository forumPostRepository
            , IForumCommentRepository forumCommentRepository, IMapper mapper, IMediator mediator)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _reportRepository = reportRepository;
            _forumPostRepository = forumPostRepository;
            _forumCommentRepository = forumCommentRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponse> Handle(CreateReportCommand request, CancellationToken cancellationToken)
        {
            var targetId = GetTargetId(request);
            var exists = await _reportRepository.ExistsAsync(request.UserId, request.Type, targetId);
            if (exists)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "You have already reported this item"
                };
            }
            if (request.Type == ReportTypeStatus.NovelReport)
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                {
                    return new ApiResponse { Success = false, Message = "Novel not found" };
                }
            }
            else if (request.Type == ReportTypeStatus.ChapterReport)
            {
                var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
                if (chapter == null)
                {
                    return new ApiResponse { Success = false, Message = "Chapter not found" };
                }
            }
            else if (request.Type == ReportTypeStatus.CommentReport)
            {
                var comment = await _commentRepository.GetByIdAsync(request.CommentId);
                if (comment == null)
                {
                    return new ApiResponse { Success = false, Message = "Comment not found" };
                }
            }
            else if (request.Type == ReportTypeStatus.UserReport)
            {
                var user = await _userRepository.GetById(request.MemberId);
                if (user == null)
                {
                    return new ApiResponse { Success = false, Message = "User not found" };
                }
            }
            else if (request.Type == ReportTypeStatus.ForumPostReport)
            {
                var forumPost = await _forumPostRepository.GetByIdAsync(request.ForumPostId);
                if (forumPost == null)
                {
                    return new ApiResponse { Success = false, Message = "Forum post not found" };
                }
            }   
            else if (request.Type == ReportTypeStatus.ForumCommentReport)
            {
                var forumComment = await _forumCommentRepository.GetByIdAsync(request.ForumCommentId);
                if (forumComment == null)
                {
                    return new ApiResponse { Success = false, Message = "Forum comment not found" };
                }
            }
            var createRequest = new ReportEntity
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                type = request.Type,
                chapter_id = request.ChapterId,
                novel_id = request.NovelId,
                member_id = request.MemberId,
                comment_id = request.CommentId,
                forum_post_id = request.ForumPostId,
                forum_comment_id = request.ForumCommentId,
                reason = request.Reason,
                status = ReportStatus.InProgress,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            await _reportRepository.CreateAsync(createRequest);
            var response = _mapper.Map<ReportResponse>(createRequest);

            var admin = await _userRepository.GetFirstUserByRoleAsync(Role.Admin);
            NotificationType notiType = request.Type switch
            {
                ReportTypeStatus.ChapterReport => NotificationType.ChapterReportNotification,
                ReportTypeStatus.NovelReport => NotificationType.NovelReportNofitication,
                ReportTypeStatus.CommentReport => NotificationType.ReportComment,
                ReportTypeStatus.UserReport => NotificationType.UserReport,
                _ => NotificationType.UserReport
            };


            await _mediator.Send(new SendNotificationToUserCommand
            {
                UserId = admin.id,
                SenderId = request.UserId,
                NovelId = request.NovelId,
                ChapterId = request.ChapterId,
                CommentId = request.CommentId,
                UserReportedId = request.MemberId,
                Type = notiType
            });

            return new ApiResponse
            {
                Success = true,
                Message = "Report created successfully",
                Data = new
                {
                    Comment = response,
                    SignalR = new
                    {
                        Sent = true,
                        NotificationType = notiType.ToString(),
                    }
                }
            };
        }

        private string GetTargetId(CreateReportCommand request)
        {
            var result = request;
            return result.Type switch
            {
                ReportTypeStatus.UserReport => result.MemberId,
                ReportTypeStatus.NovelReport => result.NovelId,
                ReportTypeStatus.ChapterReport => result.ChapterId,
                ReportTypeStatus.CommentReport => result.CommentId,
                ReportTypeStatus.ForumPostReport => result.ForumPostId,
                ReportTypeStatus.ForumCommentReport => result.ForumCommentId,
                _ => string.Empty
            };
        }
    }
}
