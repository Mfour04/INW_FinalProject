using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Novel.Commands
{
    public class BuyNovelCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public string? NovelId { get; set; }
        public int CoinCost { get; set; }
    }

    public class BuyNovelCommandHandler : IRequestHandler<BuyNovelCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IPurchaserRepository _purchaserRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;
        private readonly IAuthorEarningRepository _authorEarningRepo;

        public BuyNovelCommandHandler(
           IUserRepository userRepo,
           IPurchaserRepository purchaserRepo,
           ITransactionRepository transactionRepo,
           INovelRepository novelRepo,
           IChapterRepository chapterRepo,
           IAuthorEarningRepository authorEarningRepo)
        {
            _userRepo = userRepo;
            _purchaserRepo = purchaserRepo;
            _transactionRepo = transactionRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
            _authorEarningRepo = authorEarningRepo;
        }

        public async Task<ApiResponse> Handle(BuyNovelCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.NovelId))
                return Fail("Thiếu ID người dùng hoặc truyện.");

            var novel = await _novelRepo.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return Fail("Không tìm thấy truyện.");

            if (!novel.is_paid)
                return Fail("Truyện này miễn phí và không cần mua.");

            if (novel.status != NovelStatus.Completed)
                return Fail("Chỉ những truyện hoàn thành mới có thể mua trọn bộ.");

            var existing = await _purchaserRepo.GetByUserAndNovelAsync(request.UserId, request.NovelId);
            if (existing?.is_full == true)
                return Fail("Người dùng đã sở hữu truyện này.");

            var user = await _userRepo.GetById(request.UserId);
            if (user == null)
                return Fail("Không tìm thấy người dùng.");
            if (user.coin < request.CoinCost)
                return Fail("Số coin không đủ.");

            var chapterIds = await _chapterRepo.GetIdsByNovelIdAsync(request.NovelId);
            var nowTicks = TimeHelper.NowTicks;

            TransactionEntity transaction = new()
            {
                id = SystemHelper.RandomId(),
                requester_id = request.UserId,
                novel_id = request.NovelId,
                type = PaymentType.BuyNovel,
                amount = request.CoinCost,
                payment_method = "Coin",
                status = PaymentStatus.Completed,
                created_at = nowTicks,
                completed_at = nowTicks
            };

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
                return Fail("Không trừ được tiền xu.");

            await _transactionRepo.AddAsync(transaction);

            if (!string.IsNullOrEmpty(novel.author_id))
            {
                await _userRepo.IncreaseCoinAsync(novel.author_id, request.CoinCost);

                AuthorEarningEntity authorEarning = new()
                {
                    id = SystemHelper.RandomId(),
                    author_id = novel.author_id,
                    novel_id = novel.id,
                    amount = request.CoinCost,
                    type = PaymentType.BuyNovel,
                    source_transaction_id = transaction.id,
                    created_at = nowTicks
                };
                await _authorEarningRepo.AddAsync(authorEarning);
            }

            if (existing == null)
            {
                // Chưa có → tạo mới
                PurchaserEntity newPurchaser = new()
                {
                    id = SystemHelper.RandomId(),
                    user_id = request.UserId,
                    novel_id = request.NovelId,
                    is_full = true,
                    chap_snapshot = chapterIds.Count,
                    chapter_ids = chapterIds,
                    created_at = nowTicks
                };
                await _purchaserRepo.CreateAsync(newPurchaser);

                return new ApiResponse
                {
                    Success = true,
                    Message = "Đã mua tiểu thuyết thành công.",
                    Data = new { Purchaser = newPurchaser, Transaction = transaction }
                };
            }

            existing.is_full = true;
            existing.chap_snapshot = chapterIds.Count;
            existing.chapter_ids = chapterIds;
            await _purchaserRepo.UpdateAsync(existing.id, existing);

            return new ApiResponse
            {
                Success = true,
                Message = "Đã nâng cấp lên mua toàn bộ tiểu thuyết.",
                Data = new { Purchaser = existing, Transaction = transaction }
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}