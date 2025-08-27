namespace Shared.Contracts.Response.AuthorAnaysis
{
    public class AuthorEarningsSummaryResponse
    {
        public int TotalEarningsCoins { get; set; }
        public int TotalOrders { get; set; }
        public string FilterApplied { get; set; } = "all";
        public string? NovelId { get; set; }
        public int TotalLogs { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
        public List<AuthorEarningPurchaseLogItem> Logs { get; set; } = new();
    }

    public class AuthorEarningPurchaseLogItem
    {
        public string EarningId { get; set; }
        public string NovelId { get; set; }
        public string NovelTitle { get; set; } 
        public string? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public string Type { get; set; }
        public int Amount { get; set; }
        public long CreatedAt { get; set; }
        public string BuyerUsername { get; set; }
        public string BuyerDisplayName { get; set; }
    }

    public class AuthorEarningsChartResponse
    {
        public string Label { get; set; } = default!;
        public int Coins { get; set; }
        public long BucketStartTicks { get; set; }
        public long BucketEndTicks { get; set; }
    }

    public class AuthorTopNovelResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public int TotalCoins { get; set; }
        public int TotalOrders { get; set; }
    }
}