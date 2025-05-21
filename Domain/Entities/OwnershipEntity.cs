using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class OwnershipEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string user_id { get; set; }
        public string novel_id { get; set; }
        public List<string> chapter_id { get; set; } = new();
        public bool is_full { get; set; }
    }
}