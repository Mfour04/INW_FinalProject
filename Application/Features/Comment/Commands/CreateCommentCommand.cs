using Application.Features.Notification.Commands;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Comment;
using Shared.Helpers;

namespace Application.Features.Comment.Commands
{
    public class CreateCommentCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public string? NovelId { get; set; }
        public string? ChapterId { get; set; }
        public string Content { get; set; }
        public string? ParentCommentId { get; set; }
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ApiResponse>
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICommentSpamGuard _spamGuard;

        public CreateCommentCommandHandler(
            ICommentRepository commentRepo,
            IChapterRepository chapterRepo,
            INovelRepository novelRepo,
            IUserRepository userRepo,
            IMapper mapper,
            IMediator mediator,
            ICommentSpamGuard spamGuard)
        {
            _commentRepo = commentRepo;
            _chapterRepo = chapterRepo;
            _novelRepo = novelRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _mediator = mediator;
            _spamGuard = spamGuard;
        }

        public async Task<ApiResponse> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return Fail("Content is required.");

            bool isReply = !string.IsNullOrWhiteSpace(request.ParentCommentId);

            // Nếu là reply comment
            if (isReply)
            {
                var parentComment = await _commentRepo.GetByIdAsync(request.ParentCommentId);
                if (parentComment == null)
                    return Fail("Parent comment not found.");

                request.NovelId = parentComment.novel_id;
                request.ChapterId = parentComment.chapter_id;
            }
            else
            {
                // Nếu là comment gốc yêu cầu NovelId hoặc ChapterId
                if (string.IsNullOrWhiteSpace(request.NovelId) && string.IsNullOrWhiteSpace(request.ChapterId))
                    return Fail("Either NovelId or ChapterId must be provided.");

                if (!string.IsNullOrWhiteSpace(request.ChapterId))
                {
                    var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId);
                    if (chapter == null)
                        return Fail("Chapter not found.");

                    var foundNovel = await _novelRepo.GetByNovelIdAsync(chapter.novel_id);
                    if (foundNovel == null)
                        return Fail("Novel not found.");

                    request.NovelId = foundNovel.id;
                }

                var novel = await _novelRepo.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                    return Fail("Novel not found.");
            }

            bool isChapter = !string.IsNullOrWhiteSpace(request.ChapterId);
            bool isNovel = !string.IsNullOrWhiteSpace(request.NovelId);

            var spamResult = await _spamGuard.CheckSpamAsync(request.UserId!, request.NovelId, request.ChapterId, request.Content);
            if (spamResult != null)
                return spamResult;

            CommentEntity comment = new()
            {
                id = SystemHelper.RandomId(),
                novel_id = request.NovelId,
                chapter_id = request.ChapterId,
                user_id = request.UserId,
                content = request.Content,
                content_hash = SystemHelper.ComputeSha256(request.Content.Trim().ToLower()),
                parent_comment_id = request.ParentCommentId,
                created_at = TimeHelper.NowTicks
            };

            await _commentRepo.CreateAsync(comment);

            NotificationType notiType = (isReply, isChapter, isNovel) switch
            {
                (false, false, true) => NotificationType.CommentNovelNotification,
                (false, true, true) => NotificationType.CommentChapterNotification,
                (true, false, true) => NotificationType.RelyCommentNovel,
                (true, true, true) => NotificationType.RelyCommentChapter,
                _ => throw new Exception("Invalid combination of comment context.")
            };

            var notiResponse = await _mediator.Send(new SendNotificationToUserCommand
            {
                SenderId = request.UserId,
                NovelId = request.NovelId,
                ChapterId = request.ChapterId,
                CommentId = comment.id,
                ParentCommentId = request.ParentCommentId,
                Type = notiType
            });

            bool signalRSent = notiResponse.Success;

            if (isChapter)
                await _chapterRepo.IncrementCommentsAsync(request.ChapterId);
            else
                await _novelRepo.IncrementCommentsAsync(request.NovelId);

            var response = _mapper.Map<CreateCommentResponse>(comment);
            var user = await _userRepo.GetById(request.UserId);

            response.Author = new BaseCommentResponse.UserInfo
            {
                Id = user.id,
                UserName = user.username,
                DisplayName = user.displayname,
                Avatar = user.avata_url
            };

            response.SignalR = new CreateCommentResponse.SignalRResult
            {
                Sent = signalRSent,
                NotificationType = notiType.ToString()
            };

            return new ApiResponse
            {
                Success = true,
                Message = "Comment created successfully and SignalR sent.",
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
