namespace Domain.Entities
{
    public class ChapterEntity : BaseEntity
    {
        public string novel_id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int chapter_number { get; set; }
        public bool is_paid { get; set; }
        public int price { get; set; }
    }
}
