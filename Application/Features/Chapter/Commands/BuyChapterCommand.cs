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
        private readonly IAuthorEarningRepository _authorEarningRepo; 

         public BuyChapterCommandHandler(
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

        public async Task<ApiResponse> Handle(BuyChapterCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ChapterId))
                return Fail("Thiếu ID người dùng hoặc ID chương.");

            var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId);
            if (chapter == null)
                return Fail("Không tim thấy chương.");
            if (!chapter.is_paid)
                return Fail("Chương này miễn phí và không cần phải mua.");

            var novel = await _novelRepo.GetByNovelIdAsync(chapter.novel_id);
            if (novel == null)
                return Fail("Không tìm thấy truyện.");

            if (await _purchaserRepo.HasPurchasedChapterAsync(request.UserId, novel.id, request.ChapterId))
                return Fail("Chương này bạn đã mua rồi.");

            var existing = await _purchaserRepo.GetByUserAndNovelAsync(request.UserId, novel.id);
            if (existing?.is_full == true)
                return Fail("Bạn đã mua trọn bộ truyện này.");

            var user = await _userRepo.GetById(request.UserId);
            if (user == null || user.coin < request.CoinCost)
                return Fail("Số coin không đủ.");

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
                return Fail("Trừ coin thất bại.");

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

            if (!string.IsNullOrEmpty(novel.author_id))
            {
                await _userRepo.IncreaseCoinAsync(novel.author_id, request.CoinCost);

                AuthorEarningEntity authorEarning = new()
                {
                    id = SystemHelper.RandomId(),
                    author_id = novel.author_id,
                    novel_id = novel.id,
                    chapter_id = request.ChapterId,
                    amount = request.CoinCost,
                    type = PaymentType.BuyChapter,
                    source_transaction_id = transaction.id,
                    created_at = TimeHelper.NowTicks
                };

                await _authorEarningRepo.AddAsync(authorEarning);
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Mua chương truyện thành công.",
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