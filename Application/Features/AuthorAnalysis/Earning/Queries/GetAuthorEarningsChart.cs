using Application.Services.Interfaces;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.AuthorAnaysis;

namespace Application.Features.AuthorAnalysis.Earning.Queries
{
    public class GetAuthorEarningsChart : IRequest<ApiResponse>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; } = "day";
        public string Filter { get; set; } = "all";
        public string? NovelId { get; set; }
    }

    public class GetAuthorEarningsChartHandler : IRequestHandler<GetAuthorEarningsChart, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;
        private readonly ICurrentUserService _currentUser;

        public GetAuthorEarningsChartHandler(
            IAuthorEarningRepository authorEarningRepo,
            ICurrentUserService currentUser)
        {
            _authorEarningRepo = authorEarningRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAuthorEarningsChart request, CancellationToken cancellationToken)
        {
            long startTicks = request.StartDate?.Date.Ticks ?? 0L;
            long endTicks = request.EndDate?.Date.AddDays(1).AddTicks(-1).Ticks ?? long.MaxValue;

            var raws = string.IsNullOrWhiteSpace(request.NovelId)
               ? await _authorEarningRepo.GetByAuthorIdAsync(_currentUser.UserId!, startTicks, endTicks)
               : await _authorEarningRepo.GetByNovelAsync(_currentUser.UserId!, request.NovelId!, startTicks, endTicks);

            var f = (request.Filter ?? "all").Trim().ToLowerInvariant();
            IEnumerable<Domain.Entities.AuthorEarningEntity> rows = f switch
            {
                "novel" => raws.Where(x => x.type == PaymentType.BuyNovel),
                "chapter" => raws.Where(x => x.type == PaymentType.BuyChapter),
                _ => raws
            };

            string gb = (request.GroupBy ?? "day").Trim().ToLowerInvariant();

            var grouped = rows
                 .GroupBy(x =>
                 {
                     var dtLocal = new DateTime(x.created_at); 
                     DateTime bucketStart = gb switch
                     {
                         "month" => new DateTime(dtLocal.Year, dtLocal.Month, 1, 0, 0, 0),
                         "year" => new DateTime(dtLocal.Year, 1, 1, 0, 0, 0),
                         _ => dtLocal.Date
                     };
                     DateTime bucketEnd = gb switch
                     {
                         "month" => new DateTime(dtLocal.Year, dtLocal.Month, 1).AddMonths(1).AddTicks(-1),
                         "year" => new DateTime(dtLocal.Year, 1, 1).AddYears(1).AddTicks(-1),
                         _ => bucketStart.AddDays(1).AddTicks(-1)
                     };

                     return (Start: bucketStart.Ticks, End: bucketEnd.Ticks);
                 })
                 .Select(g => new
                 {
                     g.Key.Start,
                     g.Key.End,
                     Coins = g.Sum(x => x.amount)
                 })
                 .OrderBy(x => x.Start)
                 .Select(x =>
                 {
                     var start = new DateTime(x.Start);
                     var label = gb switch
                     {
                         "month" => start.ToString("MM/yyyy"),
                         "year" => start.Year.ToString(),
                         _ => start.ToString("yyyy-MM-dd")
                     };

                     return new AuthorEarningsChartResponse
                     {
                         Label = label,
                         Coins = x.Coins,
                         BucketStartTicks = x.Start,
                         BucketEndTicks = x.End
                     };
                 })
                 .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved author earnings chart successfully.",
                Data = grouped
            };
        }
    }
}