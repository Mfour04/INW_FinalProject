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
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ChapterId))
                return Fail("Missing user or chapter ID.");

            var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return Fail("Chapter not found.");
            if (!chapter.is_paid)
                return Fail("This chapter is free and does not need to be purchased.");

            var novel = await _novelRepo.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
                return Fail("Novel not found.");

            if (await _purchaserRepo.HasPurchasedChapterAsync(request.UserId, novel.id, request.ChapterId))
                return Fail("Chapter already purchased.");

			var existing = await _purchaserRepo.GetByUserAndNovelAsync(request.UserId, novel.id);
			if (existing?.is_full == true)
				return Fail("You have already purchased the full novel.");

			var user = await _userRepo.GetById(request.UserId);
            if (user == null || user.coin < request.CoinCost)
                return Fail("Not enough coins.");

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
                return Fail("Failed to deduct coins.");

            if (existing == null)
            {
                PurchaserEntity newPurchaser = new()
                {
                    id = SystemHelper.RandomId(),
                    user_id = request.UserId,
                    novel_id = novel.id,
                    is_full = false,
                    chapter_ids = new List<string> { request.ChapterId },
                    chap_snapshot = 1,
                    created_at = TimeHelper.NowTicks
                };
                await _purchaserRepo.CreateAsync(newPurchaser);
            }
            else
            {
                await _purchaserRepo.AddChapterAsync(request.UserId!, novel.id, request.ChapterId);
            }

            TransactionEntity transaction = new()
            {
                id = SystemHelper.RandomId(),
                requester_id = request.UserId,
                novel_id = novel.id,
                chapter_id = request.ChapterId,
                type = PaymentType.BuyChapter,
                amount = request.CoinCost,
                payment_method = "Coin",
                status = PaymentStatus.Completed,
                created_at = TimeHelper.NowTicks,
                completed_at = TimeHelper.NowTicks
            };
            await _transactionRepo.AddAsync(transaction);


            return new ApiResponse
            {
                Success = true,
                Message = "Purchase chapter successfully.",
                Data = new { Transaction = transaction }
            };
        }

        private ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}