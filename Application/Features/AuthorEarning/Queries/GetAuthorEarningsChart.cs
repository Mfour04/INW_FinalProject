using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;

namespace Application.Features.AuthorEarning.Queries
{
    public class GetAuthorEarningsChart : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; } = "day";
    }
    
    public class GetAuthorEarningsChartHandler : IRequestHandler<GetAuthorEarningsChart, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;

        public GetAuthorEarningsChartHandler(IAuthorEarningRepository authorEarningRepo)
        {
            _authorEarningRepo = authorEarningRepo;
        }

        public async Task<ApiResponse> Handle(GetAuthorEarningsChart request, CancellationToken cancellationToken)
        {
            var startTicks = request.StartDate?.Ticks ?? 0;
            var endTicks = request.EndDate?.Ticks ?? long.MaxValue;

            var earnings = await _authorEarningRepo.GetByAuthorIdAsync(request.UserId!, startTicks, endTicks);

            var groupedData = earnings
                .GroupBy(x =>
                {
                    var date = new DateTime(x.created_at);
                    return request.GroupBy.ToLower() switch
                    {
                        "month" => new DateTime(date.Year, date.Month, 1),
                        "year" => new DateTime(date.Year, 1, 1),
                        _ => date.Date 
                    };
                })
                .Select(g => new AuthorEarningsChartResponse
                {
                    Label = request.GroupBy.ToLower() switch
                    {
                        "month" => g.Key.ToString("MM/yyyy"),
                        "year" => g.Key.Year.ToString(),
                        _ => g.Key.ToString("yyyy-MM-dd")
                    },
                    Coins = g.Sum(x => x.amount)
                })
                .OrderBy(x => x.Label)
                .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved author earnings chart successfully.",
                Data = groupedData
            };
        }
    }
}