using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Chapter.Queries
{
    public class GetAllChapterByNovelId: IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string UserId { get; set; }
    }
    public class GetAllChapterByNovelIdHandler : IRequestHandler<GetAllChapterByNovelId, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;

        public GetAllChapterByNovelIdHandler(INovelRepository novelRepository, IChapterRepository chapterRepository, IPurchaserRepository purchaserRepository)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
        }

        public async Task<ApiResponse> Handle(GetAllChapterByNovelId request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NovelId))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "NovelId is required.",
                    Data = null
                };
            }

            var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Novel not found.",
                    Data = null
                };
            }

            var allChapters = await _chapterRepository.GetAllByNovelIdAsync(request.NovelId);

            return new ApiResponse
            {
                Success = true,
                Message = "Chapters retrieved with user permissions.",
                Data = allChapters
            };
        }
    }
}
