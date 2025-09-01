using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.OpenAIEntity;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Helpers;

namespace Application.Features.Chapter.Commands
{
    public class CreateChapterCommand : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public long? ScheduleAt { get; set; }
        public bool? IsPaid { get; set; }
        public int? Price { get; set; }
        public bool? AllowComment { get; set; }
        public bool? IsDraft { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class CreateChapterHandler : IRequestHandler<CreateChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        private readonly IOpenAIService _openAIService;
        private readonly IOpenAIRepository _openAIRepository;
        private readonly INovelFollowRepository _novelFollowRepository;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        public CreateChapterHandler(IChapterRepository chapterRepository, IMapper mapper
            , INovelRepository novelRepository, IOpenAIRepository openAIRepository, IOpenAIService openAIService
            , INovelFollowRepository novelFollowRepository, IMediator mediator, INotificationService notificationService)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
            _novelFollowRepository = novelFollowRepository;
            _mediator = mediator;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy novel này" };

            var nowTicks = TimeHelper.NowTicks;
            var isDraft = request.IsDraft ?? true;
            var isPublic = request.IsPublic ?? false;
            var today = TimeHelper.NowVN;
            long? scheduleAt = null;
            if (!isDraft && request.ScheduleAt.HasValue)
            {
                scheduleAt = request.ScheduleAt.Value;
            }

            var isScheduled = !isDraft && !isPublic && scheduleAt > nowTicks;
            var hasSchedule = !isDraft && !isPublic && scheduleAt > 0;
            if (hasSchedule)
            {
                var scheduledDate = TimeHelper.FromTicks(scheduleAt.Value);
                if (scheduledDate <= today)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Ngày lên lịch xuất bản chỉ được từ ngày tiếp theo trở đi. Vui lòng chọn ngày từ ngày mai trở đi. Nếu bạn vẫn chọn ngày xuất bản giống với ngày hiện tại thì nên chọn xuất bản ngay!"
                    };
                }
            }
            var chapter = new ChapterEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                title = request.Title,
                content = request.Content,
                chapter_number = null,
                is_paid = request.IsPaid ?? false,
                price = request.Price ?? 0,
                scheduled_at = scheduleAt ?? null,
                is_lock = false,
                is_draft = isDraft,
                is_public = isPublic,
                allow_comment = request.AllowComment ?? true,
                comment_count = 0,
                created_at = nowTicks
            };
            if (isScheduled)
            {
                chapter.is_draft = false;
                chapter.is_public = false;
            }
            else if (!isDraft && isPublic)
            {
                chapter.is_draft = false;
                chapter.is_public = true;

                var lastChapter = await _chapterRepository.GetLastPublishedAsync(chapter.novel_id);
                chapter.chapter_number = (lastChapter?.chapter_number ?? 0) + 1;
                if (chapter.is_paid && !novel.is_paid)
                {
                    novel.is_paid = true;
                    await _novelRepository.UpdateIsPaidAsync(novel.id, true);
                }

            }
            else
            {
                chapter.is_draft = true;
                chapter.is_public = false;
            }

            // ✅ Chỉ lưu 1 lần duy nhất
            await _chapterRepository.CreateAsync(chapter);
            var response = _mapper.Map<CreateChapterResponse>(chapter);
            // Update số chương nếu xuất bản ngay
            if (!chapter.is_draft && chapter.is_public)
            {
                await _novelRepository.UpdateTotalChaptersAsync(chapter.novel_id);
                await _novelRepository.UpdateNovelPriceAsync(chapter.novel_id);

                var publicChapters = await _chapterRepository.GetPublishedByNovelIdAsync(chapter.novel_id);
                if (publicChapters.Count == 1 && !novel.is_public)
                {
                    novel.is_public = true;
                    await _novelRepository.UpdateNovelAsync(novel);
                }
                // Lấy danh sách follower
                var userfollowNovel = await _novelFollowRepository.GetFollowersByNovelIdAsync(novel.id);

                // Loại bỏ tác giả và loại bỏ trùng user_id
                var distinctFollowers = userfollowNovel
                    .Select(f => f.user_id)
                    .Where(uid => uid != novel.author_id)
                    .Distinct()
                    .ToList();
                // Gửi thông báo đến tất cả người theo dõi trừ tác giả
                var message = $"Chương truyện mới: {chapter.title} của novel {novel.title} đã được phát hành.";
                await _notificationService.SendNotificationToUsersAsync(
                    distinctFollowers,
                    message,
                    NotificationType.CreateChapter,
                    novelId: chapter.novel_id,
                    novelSlug: novel.slug);

                if (!string.IsNullOrWhiteSpace(chapter.content))
                {
                    try
                    {
                        var embedding = await _openAIService.GetEmbeddingAsync(new List<string> { chapter.content });
                        var embeddingEntity = new ChapterContentEmbeddingEntity
                        {
                            chapter_id = chapter.id,
                            novel_id = novel.id,
                            vector_chapter_content = embedding[0],
                            updated_at = TimeHelper.NowTicks
                        };
                        await _openAIRepository.SaveChapterContentEmbeddingAsync(embeddingEntity);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi embedding cho chapter {chapter.id}: {ex.Message}");
                        // Optional: ghi log, không throw
                    }
                }
                return new ApiResponse
                {
                    Success = true,
                    Message = "Tạo chương thành công",
                    Data = new
                    {
                        Chapter = response,
                        SignalRTest = new
                        {
                            SentToUsers = distinctFollowers.Count,
                            SentUserIds = distinctFollowers,
                            NotificationType = NotificationType.CreateChapter.ToString(),
                            NotificationMessage = $"Tiểu thuyết \"{novel.title}\" vừa có chương mới: {chapter.title}"
                        }
                    }
                };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Tạo chương thành công",
                Data = new
                {
                    Chapter = response
                }
            };

        }
    }
}
