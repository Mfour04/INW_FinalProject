namespace Shared.Contracts.Response.User
{
    public class AuthorEarningsSummaryResponse
    {
        public int TotalEarningsCoins { get; set; }
        public int NovelSalesCount { get; set; }
        public int ChapterSalesCount { get; set; }
        public int NovelCoins { get; set; }
        public int ChapterCoins { get; set; }
    }

    public class AuthorEarningsChartResponse
    {
        public string Label { get; set; }
        public int Coins { get; set; }
    }

    public class AuthorTopEarningNovelsResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public int TotalCoins { get; set; }
        public int NovelSalesCount { get; set; }
        public int NovelCoins { get; set; }

        public int ChapterSalesCount { get; set; }
        public int ChapterCoins { get; set; }
        public List<ChapterEarningDetail> ChapterDetails { get; set; } = new();

        public class ChapterEarningDetail
        {
            public string ChapterId { get; set; }
            public int Coins { get; set; }
            public int SalesCount { get; set; }
        }
    }
}