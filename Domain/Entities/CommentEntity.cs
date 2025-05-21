namespace Domain.Entities
{
    public class CommentEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string content { get; set; }
        public string parent_comment_id { get; set; }
        public int like_count { get; set; }
    }
}
