using Domain.Enums;

namespace Domain.Entities
{
    public class ReportEntity : BaseEntity
    {
        public ReportScope scope { get; set; }

        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string comment_id { get; set; }
        public string forum_post_id { get; set; }
        public string forum_comment_id { get; set; }

        public string reporter_id { get; set; }
        public ReportReason reason { get; set; }
        public string message { get; set; }
        public ReportStatus status { get; set; }

        public ModerationAction action { get; set; }
        public string moderator_id { get; set; }
        public string moderator_note { get; set; }
        public long moderated_at { get; set; }
    }
}