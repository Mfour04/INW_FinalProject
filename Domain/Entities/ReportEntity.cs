namespace Domain.Entities
{
    public class ReportEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string comment_id { get; set; }
        public string type { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
    }
}