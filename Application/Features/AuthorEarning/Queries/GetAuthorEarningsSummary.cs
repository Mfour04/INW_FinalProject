using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;
using Shared.Helpers;

namespace Application.Features.AuthorEarning.Queries
{
    public class GetAuthorEarningsSummary : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetAuthorEarningsSummaryHandler : IRequestHandler<GetAuthorEarningsSummary, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;
        private readonly INovelRepository _novelRepo;

        public GetAuthorEarningsSummaryHandler(IAuthorEarningRepository authorEarningRepo, INovelRepository novelRepo)
        {
            _authorEarningRepo = authorEarningRepo;
            _novelRepo = novelRepo;
        }

        public async Task<ApiResponse> Handle(GetAuthorEarningsSummary request, CancellationToken cancellationToken)
        {
            var startTicks = request.StartDate?.Ticks ?? 0;
            var endTicks = request.EndDate?.Ticks ?? long.MaxValue;

            var novelIds = (await _novelRepo.GetNovelByAuthorId(request.UserId))
                .Select(x => x.id)
                .ToList();

            if (!novelIds.Any())
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "No novels found for this author.",
                };
            }

            var earnings = await _authorEarningRepo.GetByAuthorIdAsync(request.UserId!, startTicks, endTicks);

            int totalEarnings = earnings.Sum(x => x.amount);
            int novelCoins = earnings.Where(x => x.type == PaymentType.BuyNovel).Sum(x => x.amount);
            int chapterCoins = earnings.Where(x => x.type == PaymentType.BuyChapter).Sum(x => x.amount);

            int novelSalesCount = earnings.Count(x => x.type == PaymentType.BuyNovel);
            int chapterSalesCount = earnings.Count(x => x.type == PaymentType.BuyChapter);

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved author earnings summary successfully.",
                Data = new AuthorEarningsSummaryResponse
                {
                    TotalEarningsCoins = totalEarnings,
                    NovelSalesCount = novelSalesCount,
                    ChapterSalesCount = chapterSalesCount,
                    NovelCoins = novelCoins,
                    ChapterCoins = chapterCoins
                }
            };
        }
    }
}