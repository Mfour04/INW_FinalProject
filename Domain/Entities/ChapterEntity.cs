namespace Domain.Entities
{
    public class ChapterEntity : BaseEntity
    {
        public string novel_id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int? chapter_number { get; set; }
        public bool is_paid { get; set; }
        public int price { get; set; }
        public long scheduled_at { get; set; }
        public bool allow_comment { get; set; }
        public bool is_lock { get; set; }
        public bool is_draft { get; set; }
        public bool is_public { get; set; }
        public int? comment_count { get; set; }
        public int total_chapter_views { get; set; }
    }
}
