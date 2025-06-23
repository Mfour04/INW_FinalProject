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

        public BuyNovelCommandHandler(
            IUserRepository userRepo,
            IPurchaserRepository purchaserRepo,
            ITransactionRepository transactionRepo)
        {
            _userRepo = userRepo;
            _purchaserRepo = purchaserRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<ApiResponse> Handle(BuyNovelCommand request, CancellationToken cancellationToken)
        {
            if (await _purchaserRepo.HasPurchasedAsync(request.UserId, request.NovelId))
            {
                return Fail("User already owns this novel.");
            }

            var user = await _userRepo.GetById(request.UserId);
            if (user.coin < request.CoinCost)
            {
                return Fail("Not enough coins.");
            }

            if (!await _userRepo.DecreaseCoinAsync(request.UserId, request.CoinCost))
            {
                return Fail("Failed to deduct coins.");
            }

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
            await _transactionRepo.AddAsync(transaction);

            PurchaserEntity purchaser = new()
            {
                id = SystemHelper.RandomId(),
                user_id = request.UserId,
                novel_id = request.NovelId,
                is_full = true,
                created_at = nowTicks
            };
            await _purchaserRepo.PurchaseNovelAsync(purchaser);

            return new ApiResponse
            {
                Success = true,
                Message = "Purchase novel successfully.",
                Data = new
                {
                    Purchaser = purchaser,
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