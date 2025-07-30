using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Shared.Contracts.Response;
using Shared.Contracts.Response.User;

namespace Application.Features.AuthorEarning.Queries
{
    public class GetAuthorTopEarningNovels : IRequest<ApiResponse>
    {
        public string? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Limit { get; set; } = 3;
    }

    public class GetAuthorTopEarningNovelsHandler : IRequestHandler<GetAuthorTopEarningNovels, ApiResponse>
    {
        private readonly IAuthorEarningRepository _authorEarningRepo;
        private readonly INovelRepository _novelRepo;

        public GetAuthorTopEarningNovelsHandler(
            IAuthorEarningRepository authorEarningRepo,
            INovelRepository novelRepo)
        {
            _authorEarningRepo = authorEarningRepo;
            _novelRepo = novelRepo;
        }

        public async Task<ApiResponse> Handle(GetAuthorTopEarningNovels request, CancellationToken cancellationToken)
        {
            var startTicks = request.StartDate?.Ticks ?? 0;
            var endTicks = request.EndDate?.Ticks ?? DateTime.MaxValue.Ticks;

            var novels = await _novelRepo.GetNovelByAuthorId(request.UserId);
            var novelMap = novels.ToDictionary(
                x => x.id,
                x => new { x.title, x.novel_image }
            );

            if (!novelMap.Any())
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "No novels found for this author.",
                    Data = new List<AuthorTopEarningNovelsResponse>()
                };
            }

            var earnings = await _authorEarningRepo.GetByAuthorIdAsync(request.UserId!, startTicks, endTicks);

            var filteredEarnings = earnings
               .Where(x => novelMap.ContainsKey(x.novel_id))
               .ToList();

            var topNovels = filteredEarnings
            .GroupBy(x => x.novel_id)
            .Select(g =>
            {
                var novelCoins = g.Where(x => x.type == PaymentType.BuyNovel).Sum(x => x.amount);
                var chapterCoins = g.Where(x => x.type == PaymentType.BuyChapter).Sum(x => x.amount);

                return new AuthorTopEarningNovelsResponse
                {
                    NovelId = g.Key,
                    Title = novelMap[g.Key].title,
                    Image = novelMap[g.Key].novel_image,
                    TotalCoins = novelCoins + chapterCoins,
                    NovelCoins = novelCoins,
                    NovelSalesCount = g.Count(x => x.type == PaymentType.BuyNovel),
                    ChapterCoins = chapterCoins,
                    ChapterSalesCount = g.Count(x => x.type == PaymentType.BuyChapter),
                    ChapterDetails = g
                        .Where(x => x.type == PaymentType.BuyChapter && !string.IsNullOrEmpty(x.chapter_id))
                        .GroupBy(x => x.chapter_id)
                        .Select(ch => new AuthorTopEarningNovelsResponse.ChapterEarningDetail
                        {
                            ChapterId = ch.Key!,
                            Coins = ch.Sum(c => c.amount),
                            SalesCount = ch.Count()
                        })
                        .OrderByDescending(c => c.Coins)
                        .ToList()
                };
            })
           .OrderByDescending(x => x.TotalCoins)
           .Take(request.Limit)
           .ToList();

            return new ApiResponse
            {
                Success = true,
                Message = "Retrieved top novels successfully.",
                Data = topNovels
            };
        }
    }
}