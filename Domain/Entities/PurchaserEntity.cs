using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class PurchaserEntity : BaseEntity
    {
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public List<string> chapter_ids { get; set; } = new();
        public int full_chap_count { get; set; }
        public bool is_full { get; set; }
    }
}