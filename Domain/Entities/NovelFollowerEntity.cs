using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class NovelFollowerEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string novel_id { get; set; }
        public string user_id { get; set; }
        public string username { get; set; }
        public long followed_at { get; set; }
    }
}
