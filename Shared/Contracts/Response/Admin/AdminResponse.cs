namespace Shared.Contracts.Response.Admin
{
    public class AdminDashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public List<WeeklyStatItem> NewUsersPerDay { get; set; }

        public int TotalNovels { get; set; }
        public List<WeeklyStatItem> NewNovelsPerDay { get; set; }
    }

    public class WeeklyStatItem
    {
        public string Day { get; set; }
        public string Weekday { get; set; }
        public int Count { get; set; }
    }
    
    public class AdminAnalysisResponse
    {
        public int TotalUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int LockedUsers { get; set; }
        public long TotalNovelViews { get; set; }
    }
}
