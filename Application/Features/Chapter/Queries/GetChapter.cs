using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities.OpenAIEntity;
using Domain.Entities.System;
using Infrastructure.Repositories.Implements;
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
        private readonly INovelRepository _novelRepository;

        public GetChapterHanlder(IChapterRepository chapterRepository, IMapper mapper
            , IOpenAIService openAIService, IOpenAIRepository openAIRepository, INovelRepository novelRepository)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
            _openAIService = openAIService;
            _openAIRepository = openAIRepository;
            _novelRepository = novelRepository;
        }
        public async Task<ApiResponse> Handle(GetChapter request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var chapters = await _chapterRepository.GetAllAsync(findCreterias);
            if (chapters == null || chapters.Count == 0)
                return new ApiResponse { Success = false, Message = "Chapter not found" };

            //var novelIds = chapters.Select(c => c.novel_id).Distinct().ToList();
            //var novels = await _novelRepository.GetManyByIdsAsync(novelIds);
            //var novelDictionary = novels.ToDictionary(
            //    n => n.id,
            //    n => new { n.title, n.slug }
            //);

            //var existingEmbeddings = new List<string>();
            //var newEmbeddings = new List<string>();

            //foreach (var chapter in chapters)
            //{
            //    if (chapter == null || string.IsNullOrWhiteSpace(chapter.content)) continue;

            //    try
            //    {
            //        var existingEntity = await _openAIRepository.GetChapterContentEmbeddingByIdAsync(chapter.id);

            //        if (existingEntity != null)
            //        {
            //            bool contentChanged = existingEntity.chapter_content != chapter.content;

            //            // update metadata
            //            if (novelDictionary.TryGetValue(chapter.novel_id, out var novelInfo))
            //            {
            //                existingEntity.novel_title = novelInfo.title;
            //                existingEntity.slug = novelInfo.slug;
            //            }
            //            else
            //            {
            //                existingEntity.novel_title = "Unknown";
            //                existingEntity.slug = "unknown-slug";
            //            }

            //            existingEntity.novel_id = chapter.novel_id;
            //            existingEntity.chapter_title = chapter.title;
            //            existingEntity.updated_at = TimeHelper.NowTicks;

            //            // nếu content thay đổi thì gọi lại AI
            //            if (contentChanged)
            //            {
            //                var embedding = await _openAIService.GetEmbeddingAsync(new List<string> { chapter.content });
            //                existingEntity.vector_chapter_content = embedding[0];
            //                existingEntity.chapter_content = chapter.content;
            //            }

            //            await _openAIRepository.UpdateChapterContentEmbeddingAsync(existingEntity);
            //            existingEmbeddings.Add(chapter.id);
            //        }
            //        else
            //        {
            //            // Chưa có entity -> tạo mới
            //            var embedding = await _openAIService.GetEmbeddingAsync(new List<string> { chapter.content });
            //            var newEntity = new ChapterContentEmbeddingEntity
            //            {
            //                chapter_id = chapter.id,
            //                novel_id = chapter.novel_id,
            //                vector_chapter_content = embedding[0],
            //                updated_at = TimeHelper.NowTicks
            //            };

            //            await _openAIRepository.SaveChapterContentEmbeddingAsync(newEntity);
            //            newEmbeddings.Add(chapter.id);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"[Embedding Error] Chapter {chapter.id}: {ex.Message}");
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
