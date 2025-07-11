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

                // Lấy danh sách các chapter của novel
                var allChapterIds = await _chapterRepository.GetChapterIdsByNovelIdAsync(request.NovelId);
                var freeChapterIds = await _chapterRepository.GetFreeChapterIdsByNovelIdAsync(request.NovelId);

                // Kiểm tra người dùng (guest, tác giả, đã mua truyện, v.v.)
                bool isGuest = string.IsNullOrEmpty(request.UserId);
                bool isAuthor = !isGuest && novel.author_id == request.UserId;
                bool hasPurchasedFull = !isGuest && await _purchaserRepository.HasPurchasedFullAsync(request.UserId, request.NovelId);
                var purchasedChapterIds = hasPurchasedFull ? allChapterIds : await _purchaserRepository.GetPurchasedChaptersAsync(request.UserId, request.NovelId);

                // Trả về lỗi nếu truyện chưa public, và người dùng không phải tác giả hoặc chưa mua full.
                if (!novel.is_public && !isAuthor && !hasPurchasedFull)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Truyện này chưa được công khai."
                    };
                }

                // Trường hợp truyện miễn phí và đã công khai
                if (!novel.is_paid && novel.is_public)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            NovelInfo = novel,
                            FreeChapters = freeChapterIds
                        }
                    };
                }

                // Nếu người dùng đã mua toàn bộ truyện hoặc là tác giả, trả về tất cả chương
                if (hasPurchasedFull || isAuthor)
                {
                    return new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            NovelInfo = novel,
                            AllChapters = allChapterIds
                        }
                    };
                }

                // Nếu người dùng chưa mua toàn bộ, chỉ trả về các chương đã mua và miễn phí
                return new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        NovelInfo = novel,
                        FreeChapters = freeChapterIds,
                        PurchasedChapters = purchasedChapterIds,
                        Message = "Bạn chưa mua toàn bộ truyện, chỉ xem được các chương đã mua và miễn phí."
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
