namespace Domain.Entities
{
    public class PurchaserEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public List<string> chapter_ids { get; set; } = new();
        public int chap_snapshot { get; set; }
        public bool is_full { get; set; }
    }
}