using Domain.Enums;

namespace Domain.Entities
{
    public class ReportEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string member_id { get; set; } 
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string comment_id { get; set; }
        public string forum_post_id { get; set; }
        public string forum_comment_id { get; set; }
        public ReportTypeStatus type { get; set; }
        public string reason { get; set; }
        public ReportStatus status { get; set; }
    }
}