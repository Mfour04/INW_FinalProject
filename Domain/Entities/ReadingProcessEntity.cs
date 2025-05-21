namespace Domain.Entities 
{
    public class ReadingProcessEntity : BaseEntity
    {
        public string novel_id { get; set; }
        public string chapter_id { get; set; }
        public string user_id { get; set; }
    }
}