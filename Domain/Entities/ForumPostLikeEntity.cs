using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class ForumPostLikeEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string post_id { get; set; }
        public string user_id { get; set; }
        public long like_at { get; set; }
    }
}