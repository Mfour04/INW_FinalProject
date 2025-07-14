using Application.Auth.Commands;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public bool? IsDraft { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class CreateChapterHandler : IRequestHandler<CreateChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        public CreateChapterHandler(IChapterRepository chapterRepository, IMapper mapper, INovelRepository novelRepository)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
        {
            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy novel này" };

            var nowTicks = DateTime.UtcNow.Ticks;
            var isDraft = request.IsDraft ?? true;
            var isPublic = request.IsPublic ?? false;
            var today = DateTime.UtcNow.Date;
            var scheduleAt = request.ScheduleAt.GetValueOrDefault(DateTime.UtcNow.Ticks);
            var isScheduled = !isDraft && !isPublic && scheduleAt > nowTicks;
            var hasSchedule = !isDraft && !isPublic && scheduleAt > 0;
            if (hasSchedule)
            {
                var scheduledDate = new DateTime(scheduleAt).Date;
                if (scheduledDate <= today)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Ngày lên lịch xuất bản chỉ được cho phép từ ngày tiếp theo trở đi. Vui lòng chọn ngày từ ngày mai trở đi. Nếu bạn vẫn chọn ngày xuất bản giống với ngày hiện tại thì nên chọn xuất bản ngay!"
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
                scheduled_at = scheduleAt,
                is_lock = false,
                is_draft = isDraft,
                is_public = isPublic, 
                comment_count = 0,
                created_at = nowTicks,
                updated_at = nowTicks
            };

            // 🟨 Trường hợp 1: Lên lịch xuất bản
            if (isScheduled)
            {
                chapter.is_draft = false;
                chapter.is_public = false;
                chapter.is_lock = true;
                // is_public = false (chưa được public cho đến khi background job xử lý)
                await _chapterRepository.CreateChapterAsync(chapter);
            }
            // 🟩 Trường hợp 2: Xuất bản ngay
            else if (!isDraft && isPublic)
            {
                chapter.is_draft = false;
                chapter.is_public = true;

                var lastChapter = await _chapterRepository.GetLastPublishedChapterAsync(chapter.novel_id);
                chapter.chapter_number = (lastChapter?.chapter_number ?? 0) + 1;

                await _chapterRepository.CreateChapterAsync(chapter);
                await _novelRepository.UpdateTotalChaptersAsync(chapter.novel_id);

                var publicChapters = await _chapterRepository.GetPublishedChapterByNovelIdAsync(chapter.novel_id);
                if (publicChapters.Count == 1 && !novel.is_public)
                {
                    novel.is_public = true;
                    await _novelRepository.UpdateNovelAsync(novel);
                }
            }
            // 🟥 Trường hợp 3: Bản nháp
            else
            {
                chapter.is_draft = true;
                chapter.is_public = false;
                await _chapterRepository.CreateChapterAsync(chapter);
            }

            var response = _mapper.Map<CreateChapterResponse>(chapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapter created successfully",
                Data = response
            };
        }
    }
}
