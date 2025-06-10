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
        [JsonPropertyName("chapter")]
        public CreateChapterResponse Chapter {get; set;}
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
            var novel = await _novelRepository.GetByNovelIdAsync(request.Chapter.NovelId);
            if (novel == null)
                return new ApiResponse { Success = false, Message = "Không tìm thấy novel này" };

            var chapter = new ChapterEntity
            {
                id = SystemHelper.RandomId(),
                novel_id = novel.id,
                title = request.Chapter.Title,
                content = request.Chapter.Content,
                chapter_number = request.Chapter.ChapterNumber,
                is_paid = request.Chapter.IsPaid ?? false,
                price = request.Chapter.Price ?? 0,
                created_at = DateTime.UtcNow.Ticks,
                updated_at = DateTime.UtcNow.Ticks
            };

            await _chapterRepository.CreateChapterAsync(chapter);
            var response = _mapper.Map<ChapterResponse>(chapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapter create succesfully",
                Data = response
            };
        }
    }
}
