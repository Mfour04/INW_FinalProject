namespace Shared.Contracts.Response.Rating
{
    public class RatingResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NovelId { get; set; }
        public int Score { get; set; }
        public string Content { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}
