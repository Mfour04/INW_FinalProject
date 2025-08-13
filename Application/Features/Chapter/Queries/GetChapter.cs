using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities.OpenAIEntity;
using Domain.Entities.System;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Chapter;
using Shared.Helpers;

namespace Application.Features.Chapter.Queries
{
    public class GetChapter: IRequest<ApiResponse>
    {
        public int Page = 0;
        public int Limit = int.MaxValue;
    }

    public class GetChapterHanlder : IRequestHandler<GetChapter, ApiResponse>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMapper _mapper;
        private readonly IOpenAIService _openAIService;
        private readonly IOpenAIRepository _openAIRepository;

        public GetChapterHanlder(IChapterRepository chapterRepository, IMapper mapper
            , IOpenAIService openAIService, IOpenAIRepository openAIRepository)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _openAIService = openAIService;
            _openAIRepository = openAIRepository;
        }
        public async Task<ApiResponse> Handle(GetChapter request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var chapters = await _chapterRepository.GetAllAsync(findCreterias);
            if (chapters == null || chapters.Count == 0)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            //int embeddedCount = 0;
            //var existingEmbeddings = new List<string>();
            //var newEmbeddings = new List<string>();
            //foreach (var chapter in chapters)
            //{
            //    if (chapter == null || string.IsNullOrWhiteSpace(chapter.content)) continue;
            //    var exists = await _openAIRepository.ChapterContentEmbeddingExistsAsync(chapter.id);
            //    if (exists)
            //    {
            //        existingEmbeddings.Add(chapter.id);
            //        continue;
            //    }
            //    try
            //    {
            //        var embedding = await _openAIService.GetEmbeddingAsync(new List<string> { chapter.content });
            //        var embeddingEntity = new ChapterContentEmbeddingEntity
            //        {
            //            chapter_id = chapter.id,
            //            vector_chapter_content = embedding[0],
            //            updated_at = TimeHelper.NowTicks
            //        };
            //        await _openAIRepository.SaveChapterContentEmbeddingAsync(embeddingEntity);
            //        newEmbeddings.Add(chapter.id);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"[Embedding Error] Chapter {chapter.id}: {ex.Message}");
            //        // Ghi log, không throw để tránh gián đoạn
            //    }
            //}
            var chapterResponse = _mapper.Map<List<ChapterResponse>>(chapters);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved chapters successfully.",
                Data = new
                {
                    Chapters = chapterResponse
                    //ExistingEmbeddings = existingEmbeddings,
                    //NewEmbeddings = newEmbeddings
                }
            };
        }
    }
}
