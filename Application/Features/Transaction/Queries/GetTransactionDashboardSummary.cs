using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;

namespace Application.Features.Transaction.Queries
{
    public class GetTransactionDashboardSummary : IRequest<ApiResponse>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetTransactionDashboardSummaryHandler : IRequestHandler<GetTransactionDashboardSummary, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;

        public GetTransactionDashboardSummaryHandler(ITransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }

        public async Task<ApiResponse> Handle(GetTransactionDashboardSummary request, CancellationToken cancellationToken)
        {
            var startTicks = request.StartDate.Ticks;
            var endTicks = request.EndDate.Ticks;

            var transactions = await _transactionRepo.GetCompletedTransactionsInRangeAsync(startTicks, endTicks);

            // Tính tổng
            int totalTransactions = transactions.Count;
            int totalRechargeCoins = transactions.Where(x => x.type == PaymentType.TopUp).Sum(x => x.amount);
            int totalWithdrawCoins = transactions.Where(x => x.type == PaymentType.WithdrawCoin).Sum(x => x.amount);

            // Profit: 3/13 phí trên withdraw
            int profitCoins = (int)(totalWithdrawCoins * 3.0 / 13.0);
            decimal profitVND = profitCoins * 1000;

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved transaction summary successfully.",
                Data = new AdminTransactionSummaryResponse
                {
                    TotalTransactions = totalTransactions,
                    TotalRechargeCoins = totalRechargeCoins,
                    TotalWithdrawCoins = totalWithdrawCoins,
                    ProfitCoins = profitCoins,
                    ProfitVND = profitVND
                }
            };
        }
    }
}