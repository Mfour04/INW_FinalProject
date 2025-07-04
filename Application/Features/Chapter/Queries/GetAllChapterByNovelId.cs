using Domain.Entities;
using Infrastructure.Repositories.Implements;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IPurchaserRepository _purchaserRepository;

        public GetAllChapterByNovelIdHandler(INovelRepository novelRepository, IChapterRepository chapterRepository, IPurchaserRepository purchaserRepository)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
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

            var allChapters = await _chapterRepository.GetAllChapterByNovelId(request.NovelId);

            // Trường hợp tất cả chương đều miễn phí
            if (allChapters.All(x => !x.is_paid))
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Chapters retrieved successfully.",
                    Data = allChapters
                };
            }

            // Trường hợp chương trả phí - kiểm tra quyền truy cập
            var visibleChapters = new List<ChapterEntity>();

            // Kiểm tra xem user đã mua toàn bộ truyện chưa
            bool hasFullPurchase = await _purchaserRepository.HasPurchasedFullAsync(request.UserId, request.NovelId);

            foreach (var chapter in allChapters)
            {
                if (!chapter.is_paid)
                {
                    visibleChapters.Add(chapter);
                }
                else
                {
                    if (hasFullPurchase)
                    {
                        visibleChapters.Add(chapter);
                    }
                    else
                    {
                        bool hasChapterAccess = await _purchaserRepository.HasPurchasedChapterAsync(
                            request.UserId, request.NovelId, chapter.id);

                        if (hasChapterAccess)
                        {
                            visibleChapters.Add(chapter);
                        }
                    }
                }
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Chapters retrieved with user permissions.",
                Data = visibleChapters
            };
        }
    }
}
