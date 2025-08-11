using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities.OpenAIEntity;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Helpers;

namespace Application.Features.Chapter.Commands
{
    public class UpdateChapterCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? ChapterNumber { get; set; }
        public bool? IsPaid { get; set; }
        public bool? AllowComment { get; set; }
        public int? Price { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool IsDraft { get; set; }
        public bool IsPublic { get; set; }
    }
    public class UpdateChapterHandler : IRequestHandler<UpdateChapterCommand, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly INovelRepository _novelRepository;
        private readonly IMapper _mapper;
        private readonly IOpenAIRepository _openAIRepository;
        private readonly IOpenAIService _openAIService;
        public UpdateChapterHandler(IChapterRepository chapterRepository, IMapper mapper
            , INovelRepository novelRepository, IOpenAIRepository openAIRepository, IOpenAIService openAIService)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
            _openAIRepository = openAIRepository;
            _openAIService = openAIService;
        }
        public async Task<ApiResponse> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            chapter.title = request.Title ?? chapter.title;
            chapter.content = request.Content ?? chapter.content;
            chapter.chapter_number = request.ChapterNumber ?? chapter.chapter_number;
            chapter.is_paid = request.IsPaid ?? chapter.is_paid;
            chapter.allow_comment = request.AllowComment ?? chapter.allow_comment;
            chapter.price = request.Price ?? chapter.price;
            if (request.ScheduledAt.HasValue)
            {
                chapter.scheduled_at = request.ScheduledAt.Value.ToUniversalTime().Ticks;
            }
            chapter.updated_at = TimeHelper.NowTicks;

            bool wasDraftBefore = chapter.is_draft;
            chapter.is_draft = request.IsDraft;
            chapter.is_public = request.IsPublic;

            // Nếu từ bản nháp chuyển thành public và chưa có chapter_number
            if (wasDraftBefore && request.IsPublic && chapter.chapter_number == null)
            {
                    var lastChapter = await _chapterRepository.GetLastPublishedAsync(chapter.novel_id);
                    chapter.chapter_number = (lastChapter?.chapter_number ?? 0) + 1;
            }
            await _chapterRepository.UpdateAsync(chapter);

            // Nếu chương này là public và không phải bản nháp → luôn cập nhật total_chapters
            if (chapter.is_public && !chapter.is_draft)
            {
                // Cập nhật total_chapters
                await _novelRepository.UpdateTotalChaptersAsync(chapter.novel_id);

                // Nếu đây là chương public đầu tiên, thì public luôn cả novel
                var publicChapters = await _chapterRepository.GetPublishedByNovelIdAsync(chapter.novel_id);
                if (publicChapters.Count == 1)
                {
                    var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
                    if (novel != null && !novel.is_public)
                    {
                        novel.is_public = true;
                        await _novelRepository.UpdateNovelAsync(novel);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(chapter.content))
            {
                try
                {
                    var embedding = await _openAIService.GetEmbeddingAsync(new List<string> { chapter.content });
                    var embeddingEntity = new ChapterContentEmbeddingEntity
                    {
                        chapter_id = chapter.id,
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

            var response = _mapper.Map<UpdateChapterResponse>(chapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapter updated successfully",
                Data = response
            };
        }
    }
}
