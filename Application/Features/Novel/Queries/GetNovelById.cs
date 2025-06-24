using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;

namespace Application.Features.Novel.Queries
{
    public class GetNovelById : IRequest<ApiResponse>
    {
        public string NovelId { get; set; }
        public string UserId { get; set; }
    }

    public class GetNovelByIdHandler : IRequestHandler<GetNovelById, ApiResponse>
    {
        private readonly INovelRepository _novelRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IPurchaserRepository _purchaserRepository;

        public GetNovelByIdHandler(
            INovelRepository novelRepository,
            IChapterRepository chapterRepository,
            IPurchaserRepository purchaserRepository)
        {
            _novelRepository = novelRepository;
            _chapterRepository = chapterRepository;
            _purchaserRepository = purchaserRepository;
        }

        public async Task<ApiResponse> Handle(GetNovelById request, CancellationToken cancellationToken)
        {
            try
            {
                var novel = await _novelRepository.GetByNovelIdAsync(request.NovelId);
                if (novel == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Novel not found"
                    };
                }

                var allChapterIds = await _chapterRepository.GetChapterIdsByNovelIdAsync(request.NovelId);
                var freeChapterIds = await _chapterRepository.GetFreeChapterIdsByNovelIdAsync(request.NovelId);

                // Kiểm tra nếu tác giả thêm chương mới thì cập nhật lại trạng thái full purchase
                await _purchaserRepository.ValidateFullPurchaseAsync(request.UserId, request.NovelId, novel.total_chapters);

                bool isAuthor = novel.author_id == request.UserId;
                bool hasPurchasedFull = await _purchaserRepository.HasPurchasedFullAsync(request.UserId, request.NovelId);
                var purchasedChapterIds = await _purchaserRepository.GetPurchasedChapterIdsAsync(request.UserId, request.NovelId);
                bool hasPurchasedAnyChapter = purchasedChapterIds.Any();

                // Nếu đã mua toàn bộ truyện, đánh dấu là đã sở hữu tất cả các chương
                if (hasPurchasedFull)
                {
                    purchasedChapterIds = allChapterIds;
                }

                // Trả về lỗi nếu truyện chưa public, và người dùng không phải tác giả hoặc chưa mua full.
                if (!novel.is_public && !isAuthor && !hasPurchasedFull)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Truyện này chưa được công khai."
                    };
                }

                // Trả về toàn bộ truyện và chương nếu truyện miễn phí và đã công khai.
                if (!novel.is_paid && novel.is_public)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            NovelInfo = novel,
                            AllChapters = allChapterIds,
                            PurchasedChapterIds = purchasedChapterIds
                        }
                    };
                }

                if (novel.is_paid)
                {
                    // Nếu truyện đã hoàn thành và người dùng chưa mua full → chỉ xem được chương miễn phí.
                    if (novel.status == NovelStatus.Completed)
                    {
                        if (!isAuthor && !hasPurchasedFull)
                        {
                            return new ApiResponse
                            {
                                Success = true,
                                Data = new
                                {
                                    NovelInfo = novel,
                                    FreeChapters = freeChapterIds,
                                    PurchasedChapterIds = purchasedChapterIds,
                                    Message = "Bạn chưa mua truyện này (đã hoàn thành). Chỉ xem được chương miễn phí."
                                }
                            };
                        }
                    }
                    // Nếu truyện đang ra và người dùng chưa mua chương nào → chỉ xem được chương miễn phí.
                    else if (novel.status == NovelStatus.Ongoing)
                    {
                        if (!isAuthor && !hasPurchasedAnyChapter)
                        {
                            return new ApiResponse
                            {
                                Success = true,
                                Data = new
                                {
                                    NovelInfo = novel,
                                    FreeChapters = freeChapterIds,
                                    PurchasedChapterIds = purchasedChapterIds,
                                }
                            };
                        }
                    }
                }

                // Trường hợp mặc định: người dùng là tác giả hoặc đã mua một phần hoặc toàn bộ truyện → trả về đầy đủ.
                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        NovelInfo = novel,
                        AllChapters = allChapterIds,
                        PurchasedChapterIds = purchasedChapterIds
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
