using AutoMapper;
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
        
        public GetChapterHanlder(IChapterRepository chapterRepository, IMapper mapper)
        {
            _chapterRepository = chapterRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse> Handle(GetChapter request, CancellationToken cancellationToken)
        {
            FindCreterias findCreterias = new();
            findCreterias.Limit = request.Limit;
            findCreterias.Page = request.Page;

            var chapter = await _chapterRepository.GetAllChapterAsync(findCreterias);
            if (chapter == null || chapter.Count == 0)
                return new ApiResponse { Success = false, Message = "Chapter not found" };
            var chapterResponse = _mapper.Map<List<ChapterResponse>>(chapter);
            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved chapters successfully.",
                Data = chapterResponse
            };
        }
    }
}
