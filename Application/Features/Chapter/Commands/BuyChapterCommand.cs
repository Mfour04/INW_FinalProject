using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Helpers;

namespace Application.Features.Chapter.Commands
{
    public class BuyChapterCommand : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public string? ChapterId { get; set; }
        public int CoinCost { get; set; }
    }

    public class BuyChapterCommandHandler : IRequestHandler<BuyChapterCommand, ApiResponse>
    {
        private readonly IUserRepository _userRepo;
        private readonly IPurchaserRepository _purchaserRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly INovelRepository _novelRepo;
        private readonly IChapterRepository _chapterRepo;

        public BuyChapterCommandHandler(
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

        public async Task<ApiResponse> Handle(BuyChapterCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _chapterRepo.GetByChapterIdAsync(request.ChapterId);
            if (chapter == null)
                return Fail("Chapter not found.");

            var novel = await _novelRepo.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
                return Fail("Novel not found.");

            var alreadyOwned = await _purchaserRepo.HasPurchasedChapterAsync(request.UserId, novel.id, request.ChapterId);
            if (alreadyOwned)
                return Fail("Chapter already purchased.");

            var user = await _userRepo.GetById(request.UserId);
            if (user == null || user.coin < request.CoinCost)
                return Fail("Not enough coins.");

            var success = await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost);
            if (!success)
                return Fail("Failed to deduct coins.");

            await _purchaserRepo.PurchasedChapterAsync(request.UserId, novel.id, request.ChapterId);

            TransactionEntity transaction = new()
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                novel_id = novel.id,
                chapter_id = request.ChapterId,
                type = PaymentType.BuyChapter,
                amount = request.CoinCost,
                payment_method = "Coin",
                status = PaymentStatus.Completed,
                created_at = DateTime.Now.Ticks,
                completed_at = DateTime.Now.Ticks
            };

            await _transactionRepo.AddAsync(transaction);

            var chapterCount = await _purchaserRepo.CountPurchasedChaptersAsync(request.UserId, novel.id);

            if (chapterCount == novel.total_chapters)
            {
                await _purchaserRepo.TryMarkFullAsync(request.UserId, novelId);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Purchase novel successfully.",
                Data = new
                {
                    Transaction = transaction
                }
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}