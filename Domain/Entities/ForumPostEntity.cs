namespace Domain.Entities
{
    public class ForumPostEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string content { get; set; }
        public List<string> img_urls { get; set; } = new();
        public int like_count { get; set; }
        public int comment_count { get; set; }
    }
}