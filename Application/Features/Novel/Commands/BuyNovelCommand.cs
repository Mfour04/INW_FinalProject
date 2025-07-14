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
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.NovelId))
                return Fail("Missing user or novel ID.");

            var novel = await _novelRepo.GetByNovelIdAsync(request.NovelId);
            if (novel == null)
                return Fail("Novel not found.");

            if (!novel.is_paid)
                return Fail("This novel is free and does not need to be purchased.");
            if (!novel.is_completed)
                return Fail("This novel is not yet completed and cannot be purchased as a whole.");

            if (!novel.is_completed)
                return Fail("Only completed novels can be purchased in full.");

            var existing = await _purchaserRepo.GetByUserAndNovelAsync(request.UserId, request.NovelId);
            if (existing?.is_full == true)
                return Fail("User already owns this novel.");

            var user = await _userRepo.GetById(request.UserId);
            if (user == null)
                return Fail("User not found.");
            if (user.coin < request.CoinCost)
                return Fail("Insufficient coins.");

            var chapterIds = await _chapterRepo.GetIdsByNovelIdAsync(request.NovelId);
            var nowTicks = DateTime.Now.Ticks;

            TransactionEntity transaction = new()
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

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
                return Fail("Failed to deduct coins.");

            await _transactionRepo.AddAsync(transaction);

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
                    Message = "Purchase novel successfully.",
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
                Message = "Upgraded to full novel purchase.",
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