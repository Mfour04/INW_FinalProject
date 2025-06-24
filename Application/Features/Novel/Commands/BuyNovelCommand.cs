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

        public BuyNovelCommandHandler(
           IUserRepository userRepo,
           IPurchaserRepository purchaserRepo,
           ITransactionRepository transactionRepo,
           INovelRepository novelRepo,
           IChapterRepository chapterRepo)
        {
            _userRepo = userRepo;
            _purchaserRepo = purchaserRepo;
            _transactionRepo = transactionRepo;
            _novelRepo = novelRepo;
            _chapterRepo = chapterRepo;
        }

        public async Task<ApiResponse> Handle(BuyNovelCommand request, CancellationToken cancellationToken)
        {
            if (await _purchaserRepo.HasPurchasedFullAsync(request.UserId, request.NovelId))
                return Fail("User already owns this novel.");

            var user = await _userRepo.GetById(request.UserId);
            if (user == null || user.coin < request.CoinCost)
                return Fail("Not enough coins.");

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
                return Fail("Failed to deduct coins.");

            var novel = await _novelRepo.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return Fail("Novel not found.");
            if (!novel.is_paid)
                return Fail("This novel is free and does not need to be purchased.");

            var nowTicks = DateTime.Now.Ticks;
            var chapterIds = await _chapterRepo.GetChapterIdsByNovelIdAsync(request.NovelId);

            var transaction = new TransactionEntity
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                novel_id = request.NovelId,
                type = PaymentType.BuyNovel,
                amount = request.CoinCost,
                payment_method = "Coin",
                status = PaymentStatus.Completed,
                created_at = nowTicks,
                completed_at = nowTicks
            };
            await _transactionRepo.AddAsync(transaction);

            var purchaser = new PurchaserEntity
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                novel_id = request.NovelId,
                is_full = true,
                full_chap_count = novel.total_chapters,
                chapter_ids = chapterIds,
                created_at = nowTicks
            };
            await _purchaserRepo.AddFullNovelPurchaseAsync(purchaser);

            return new ApiResponse
            {
                Success = true,
                Message = "Purchase novel successfully.",
                Data = new { Purchaser = purchaser, Transaction = transaction }
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}