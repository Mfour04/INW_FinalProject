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

namespace Application.Features.Chapter.Command
{
    public class CreateChapterCommand : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
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
            
            var chapter = new ChapterEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                title = request.Title,
                content = request.Content,
                chapter_number = null,
                is_paid = request.IsPaid ?? false,
                price = request.Price ?? 0,
                is_lock = false,
                is_draft = request.IsDraft ?? true,
                is_public = request.IsPublic ?? false,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            if (request.IsDraft != true && request.IsPublic == true)
            {
                var lastChapter = await _chapterRepository.GetLastPublishedChapterAsync(request.NovelId);
                chapter.chapter_number = (lastChapter?.chapter_number ?? 0) + 1;
            }
            await _chapterRepository.CreateChapterAsync(chapter);
            var response = _mapper.Map<CreateChapterResponse>(chapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapter create succesfully",
                Data = response
            };
        }
    }
}
