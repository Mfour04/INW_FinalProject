using AutoMapper;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;

namespace Application.Features.Transaction.Queries
{
    public class GetTransactionDashboardChart : IRequest<ApiResponse>
    {
        public string Range { get; set; } = "day";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetTransactionDashboardChartHandler
       : IRequestHandler<GetTransactionDashboardChart, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IMapper _mapper;

        public GetTransactionDashboardChartHandler(ITransactionRepository transactionRepo, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetTransactionDashboardChart request, CancellationToken cancellationToken)
        {
            var transactions = await _transactionRepo
                .GetCompletedTransactionsInRangeAsync(request.StartDate.Ticks, request.EndDate.Ticks);

            var grouped = transactions
                .Select(t => new
                {
                    Date = new DateTime(t.completed_at),
                    Type = t.type,
                    Amount = t.amount
                })
                .GroupBy(t =>
                {
                    return request.Range switch
                    {
                        "month" => $"{t.Date:yyyy-MM}",
                        "year" => $"{t.Date:yyyy}",
                        _ => $"{t.Date:yyyy-MM-dd}"
                    };
                })
                .Select(g => new AdminTransactionChartResponse
                {
                    Label = g.Key,

                    // Nạp coin (TopUp)
                    RechargeCount = g.Count(x => x.Type == PaymentType.TopUp),
                    RechargeCoins = g.Where(x => x.Type == PaymentType.TopUp).Sum(x => x.Amount),

                    // Rút coin (WithdrawCoin)
                    WithdrawCount = g.Count(x => x.Type == PaymentType.WithdrawCoin),
                    WithdrawCoins = g.Where(x => x.Type == PaymentType.WithdrawCoin).Sum(x => x.Amount),

                    // Lợi nhuận từ phí rút
                    ProfitCoins = (int)(g.Where(x => x.Type == PaymentType.WithdrawCoin).Sum(x => x.Amount) * 3.0 / 13.0)
                })
                .OrderBy(x => x.Label)
                .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved transaction chart successfully.",
                Data = grouped
            };
        }
    }
}