using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Chapter.Command
{
    public class UpdateChapterCommand: IRequest<ApiResponse>
    {
        public string ChapterId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? ChapterNumber { get; set; }
        public bool? IsPaid { get; set; }
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

        public UpdateChapterHandler(IChapterRepository chapterRepository, IMapper mapper, INovelRepository novelRepository)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepository.GetByChapterIdAsync(request.ChapterId);
            if (chapter == null)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            chapter.title = request.Title ?? chapter.title;
            chapter.content = request.Content ?? chapter.content;
            chapter.chapter_number = request.ChapterNumber ?? chapter.chapter_number;
            chapter.is_paid = request.IsPaid ?? chapter.is_paid;
            chapter.price = request.Price ?? chapter.price;
            if (request.ScheduledAt.HasValue)
            {
                chapter.scheduled_at = request.ScheduledAt.Value.ToUniversalTime().Ticks;
            }
            chapter.updated_at = DateTime.UtcNow.Ticks;

            if (request.IsPublic)
            {
                chapter.is_draft = false;
                chapter.is_public = true;
                if (!request.ChapterNumber.HasValue)
                    {
                    var lastChapter = await _chapterRepository.GetLastPublishedChapterAsync(chapter.novel_id);
                    chapter.chapter_number = (lastChapter?.chapter_number ?? 0) + 1;
                }

                var publicChapter = await _chapterRepository.GetPublishedChapterByNovelIdAsync(chapter.novel_id);
                if(publicChapter.Count == 0)
                {
                    var novel = await _novelRepository.GetByNovelIdAsync(chapter.novel_id);
                    if (novel != null && novel.is_public == false)
                    {
                        novel.is_public = true;
                        await _novelRepository.UpdateNovelAsync(novel);
                    }
                }
            }

            await _chapterRepository.UpdateChapterAsync(chapter);
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
