namespace Shared.Contracts.Response.Rating
{
    public sealed class RatingListKeysetResponse
    {
        public List<RatingResponse> Items { get; set; } = new();
        public bool HasMore { get; set; }
        public string? NextAfterId { get; set; }
        public long? NextAfterCreatedAt { get; set; }
    }
}