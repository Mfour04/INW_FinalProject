using System.Globalization;
using AutoMapper;
using MediatR;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Shared.Contracts.Response;
using Shared.Contracts.Response.Transaction;
using Shared.Helpers;

namespace Application.Features.Transaction.Queries
{
    public class GetTransactionDashboardChart : IRequest<ApiResponse>
    {
        public string Range { get; set; } = "day";
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    public class GetTransactionDashboardChartHandler
       : IRequestHandler<GetTransactionDashboardChart, ApiResponse>
    {
        private readonly ITransactionRepository _transactionRepo;

        public GetTransactionDashboardChartHandler(ITransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }

        private static DateTime ParseVnDateStrict(string s)
        {
            var d = DateTime.ParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
            return DateTime.SpecifyKind(d, DateTimeKind.Local);
        }

        public async Task<ApiResponse> Handle(GetTransactionDashboardChart request, CancellationToken cancellationToken)
        {
            long startTicks, endTicks;

            if (string.IsNullOrWhiteSpace(request.StartDate) && string.IsNullOrWhiteSpace(request.EndDate))
            {
                startTicks = DateTime.MinValue.Ticks;
                endTicks   = DateTime.MaxValue.Ticks;
            }
            else
            {
                var startVN = string.IsNullOrWhiteSpace(request.StartDate)
                    ? TimeHelper.NowVN.Date
                    : ParseVnDateStrict(request.StartDate).Date;

                var endVN = string.IsNullOrWhiteSpace(request.EndDate)
                    ? startVN
                    : ParseVnDateStrict(request.EndDate).Date;

                if (endVN < startVN)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "EndDate phải >= StartDate."
                    };
                }

                startTicks = startVN.Ticks;                             
                endTicks   = endVN.AddDays(1).AddTicks(-1).Ticks;        
            }

            var transactions = await _transactionRepo.GetCompletedTransactionsInRangeAsync(startTicks, endTicks);

            const long MS_THRESHOLD = 10_000_000_000L;
            DateTime ToVnDate(long value)
            {
                if (value < MS_THRESHOLD)
                {
                    var vnTicks = TimeHelper.FromUnixMillisecondsToVNTicks(value);
                    return new DateTime(vnTicks, DateTimeKind.Local);
                }
                else
                {
                    return new DateTime(value, DateTimeKind.Local);
                }
            }

            var grouped = transactions
                .Select(t => new
                {
                    Date   = ToVnDate(t.completed_at),
                    Type   = t.type,
                    Amount = t.amount
                })
                .GroupBy(t => request.Range switch
                {
                    "month" => $"{t.Date:yyyy-MM}",
                    "year"  => $"{t.Date:yyyy}",
                    _       => $"{t.Date:yyyy-MM-dd}"
                })
                .Select(g => new AdminTransactionChartResponse
                {
                    Label = g.Key,

                    // Nạp coin
                    RechargeCount = g.Count(x => x.Type == PaymentType.TopUp),
                    RechargeCoins = g.Where(x => x.Type == PaymentType.TopUp).Sum(x => x.Amount),

                    // Rút coin
                    WithdrawCount = g.Count(x => x.Type == PaymentType.WithdrawCoin),
                    WithdrawCoins = g.Where(x => x.Type == PaymentType.WithdrawCoin).Sum(x => x.Amount),

                    // Lợi nhuận từ phí rút (3/13)
                    ProfitCoins = (int)(g.Where(x => x.Type == PaymentType.WithdrawCoin).Sum(x => x.Amount) * 3.0 / 13.0),
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
