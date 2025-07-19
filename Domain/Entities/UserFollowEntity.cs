using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    public class UserFollowEntity
    {
        [BsonId]
        [BsonElement("_id")]
        public string id { get; set; }
        public string actor_id { get; set; }
        public string target_id { get; set; }
        public long followed_at { get; set; }
    }
}