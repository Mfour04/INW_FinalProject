namespace Shared.Contracts.Response.Forum
{
    public class CreatePostResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public List<string> ImgUrls { get; set; }
        public long CreatedAt { get; set; }
    }
}