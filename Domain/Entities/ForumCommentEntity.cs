namespace Domain.Entities
{
    public class ForumCommentEntity : BaseEntity
    {
        public string post_id { get; set; }
        public string user_id { get; set; }
        public string content { get; set; }
        public string parent_comment_id { get; set; }
        public int like_count { get; set; }
        public int reply_count { get; set; }
    }
}