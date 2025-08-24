namespace Shared.Contracts.Response.AuthorAnalysis
{
    public class AuthorTopViewedNovelResponse
    {
        public string NovelId { get; set; }
        public string Title { get; set; }
        public int TotalViews { get; set; }
    }
}