namespace Shared.Contracts.Response.Rating
{
    public class RatingResponse
    {
        public string RatingId { get; set; }
        public string NovelId { get; set; }
        public UserInfo Author { get; set; }
        public int Score { get; set; }
        public string Content { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }

        public class UserInfo
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Avatar { get; set; }
        }
    }
}
