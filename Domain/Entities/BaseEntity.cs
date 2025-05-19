using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class BaseEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public long created_at { get; set; }
        public long updated_at { get; set; }
    }
}
